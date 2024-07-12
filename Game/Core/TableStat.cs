using Cysharp.Threading.Tasks;
using Game.Core;
using GreenOne;
using System;

namespace Game
{
    /// <summary>
    /// Класс, представляющий характеристику типа <see cref="int"/>, изменение значения которой вызывает события перед/после изменения этой характеристики.<br/>
    /// Событие, вызываемое перед изменением, может полностью отменить это изменение.
    /// </summary>
    public sealed class TableStat : ITableLoggable, ICloneableWithArgs, IDisposable
    {
        public IIdEventVoidAsync<PreSetArgs> OnPreSet => _onPreSet;
        public IIdEventVoidAsync<PostSetArgs> OnPostSet => _onPostSet;

        public object Owner => _owner;
        public float PosValue => _posValue;
        public float PosScale => _posScale;
        public float NegValue => _negValue;

        public string TableName => _id;
        public string TableNameDebug => _id;

        readonly string _id;
        readonly object _owner;
        readonly TableEventVoid<PreSetArgs> _onPreSet;
        readonly TableEventVoid<PostSetArgs> _onPostSet;
        readonly TableEntryDict _valueEntries;
        readonly TableEntryDict _valueScaleEntries;

        float _posValue;
        float _posScale;
        float _negValue;
        float _relDelta;
        int _value;

        public class PreSetArgs
        {
            public readonly bool isRelative;
            public readonly ITableEntrySource source;
            public readonly int oldStatValue;
            public float deltaValue;
            public bool handled;

            public PreSetArgs(int oldStatValue, float deltaValue, bool isRelative, ITableEntrySource source)
            {
                this.isRelative = isRelative;
                this.source = source;
                this.oldStatValue = oldStatValue;
                this.deltaValue = deltaValue;
                this.handled = false;
            }
        }
        public class PostSetArgs
        {
            public readonly bool isRelative;
            public readonly ITableEntrySource source;
            public readonly int oldStatValue;
            public readonly int newStatValue;
            public readonly int totalDeltaValue;

            public PostSetArgs(int oldStatValue, int newStatValue, bool isRelative, ITableEntrySource source)
            {
                this.isRelative = isRelative;
                this.oldStatValue = oldStatValue;
                this.newStatValue = newStatValue;
                this.source = source;
                this.totalDeltaValue = newStatValue - oldStatValue;
            }
        }

        public TableStat(string id, object owner, int defaultValue)
        {
            _id = id;
            _owner = owner;

            _onPreSet = new TableEventVoid<PreSetArgs>();
            _onPostSet = new TableEventVoid<PostSetArgs>();

            _valueEntries = new TableEntryDict();
            _valueScaleEntries = new TableEntryDict();

            _posValue = defaultValue;
            _negValue = 0;
            _posScale = 1;
            _value = defaultValue;
        }
        private TableStat(TableStat src, TableStatCloneArgs args)
        {
            _id = (string)src._id.Clone();
            _owner = args.ownerClone;

            _onPreSet = (TableEventVoid<PreSetArgs>)src._onPreSet.Clone();
            _onPostSet = (TableEventVoid<PostSetArgs>)src._onPostSet.Clone();

            TableEntryDictCloneArgs entriesCArgs = new(args.terrCArgs);

            _valueEntries = (TableEntryDict)src._valueEntries.Clone(entriesCArgs);
            _valueScaleEntries = (TableEntryDict)src._valueScaleEntries.Clone(entriesCArgs);

            _posValue = src._posValue;
            _negValue = src._negValue;
            _posScale = src._posScale;
            _value = src._value;
        }

        public static implicit operator int(TableStat stat) => stat._value;

        public void Dispose()
        {
            _onPreSet.Clear();
            _onPostSet.Clear();
            _valueScaleEntries.Clear();
        }
        public object Clone(CloneArgs args)
        {
            if (args is TableStatCloneArgs cArgs)
                return new TableStat(this, cArgs);
            else return null;
        }
        public override string ToString()
        {
            return _value.ToString();
        }

        // use entryId and RevertValue to revert applied effect instead of calling AdjustValue with negative value
        // (as value can be modified by OnPreSet event)

        public float EntryValue(string entryId)
        {
            if (_valueEntries.TryGetValue(entryId, out TableEntry entry))
                 return entry.value;
            else return 0;
        }
        public float EntryValueScale(string entryId)
        {
            if (_valueScaleEntries.TryGetValue(entryId, out TableEntry entry))
                return entry.value;
            else return 0;
        }

        public UniTask SetValue(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value - _posValue - _negValue, source, entryId, false);
        }
        public UniTask SetValueScale(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value - _posScale, source, entryId, true);
        }

        public UniTask AdjustValue(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value, source, entryId, false);
        }
        public UniTask AdjustValueScale(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value, source, entryId, true);
        }

        public UniTask RevertValue(string entryId)
        {
            return RevertValue(entryId, false);
        }
        public UniTask RevertValueScale(string entryId)
        {
            return RevertValue(entryId, true);
        }

        async UniTask AdjustValue(float value, ITableEntrySource source, string entryId, bool isRelative)
        {
            PreSetArgs preArgs = new(_value, value, isRelative, source);
            foreach (var preSetSub in _onPreSet)
            {
                await preSetSub.Delegate.Invoke(this, preArgs);
                if (preArgs.handled) return;
            }

            float preArgsDelta = preArgs.deltaValue;
            if (preArgsDelta == 0) return;
            TableEntry entry = new(preArgsDelta, source);

            if (!isRelative)
            {
                if (preArgsDelta < 0)
                     _negValue -= preArgsDelta;
                else _posValue += preArgsDelta;
                _valueEntries.Add(entryId, entry);
            }
            else
            {
                UpdateRelValue(preArgsDelta);
                _valueScaleEntries.Add(entryId, entry);
            }

            RecalculateValue();
            await _onPostSet.Invoke(this, new PostSetArgs(preArgs.oldStatValue, _value, isRelative, source));
        }
        async UniTask RevertValue(string entryId, bool isRelative)
        {
            int oldValue = _value;
            TableEntryDict entries = isRelative ? _valueScaleEntries : _valueEntries;

            if (!entries.TryGetValue(entryId, out TableEntry entry))
                return;
            if (!isRelative)
            {
                if (entry.value < 0)
                    _negValue += entry.value;
                else _posValue -= entry.value;
            }
            else UpdateRelValue(-entry.value);

            entries.Remove(entryId);
            RecalculateValue();
            await _onPostSet.Invoke(this, new PostSetArgs(oldValue, _value, isRelative, null));
        }

        void UpdateRelValue(float delta)
        {
            _relDelta += delta;
            if (_relDelta > 0)
                 _posScale = 1 * (1 + _relDelta);
            else _posScale = 1 / (1 - _relDelta);
        }
        void RecalculateValue()
        {
            _value = (_posValue * _posScale - _negValue).Ceiling();
        }
    }
}
