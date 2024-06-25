using Cysharp.Threading.Tasks;
using GreenOne;
using System;
using Unity.Mathematics;
using static Unity.VisualScripting.Member;

namespace Game
{
    /// <summary>
    /// Класс, представляющий характеристику типа <see cref="int"/>, изменение значения которой вызывает события перед/после изменения этой характеристики.<br/>
    /// Событие, вызываемое перед изменением, может полностью отменить это изменение.
    /// </summary>
    public sealed class TableStat : ICloneableWithArgs, IDisposable
    {
        public IIdEventVoidAsync<PreSetArgs> OnPreSet => _onPreSet;
        public IIdEventVoidAsync<PostSetArgs> OnPostSet => _onPostSet;

        public object Owner => _owner;
        public float AbsValue => _absValue;
        public float RelValue => _relValue;

        readonly object _owner;
        readonly TableEventVoid<PreSetArgs> _onPreSet;
        readonly TableEventVoid<PostSetArgs> _onPostSet;
        readonly TableEntryDict _absEntries;
        readonly TableEntryDict _relEntries;

        float _absValue;
        float _relValue;
        float _relDelta;
        int _value;

        public class PreSetArgs
        {
            public readonly bool isAbsolute;
            public readonly ITableEntrySource source;
            public readonly int oldStatValue;
            public float deltaValue;
            public bool handled;

            public PreSetArgs(int oldStatValue, float deltaValue, bool isAbsolute, ITableEntrySource source)
            {
                this.isAbsolute = isAbsolute;
                this.source = source;
                this.oldStatValue = oldStatValue;
                this.deltaValue = deltaValue;
                this.handled = false;
            }
        }
        public class PostSetArgs
        {
            public readonly bool isAbsolute;
            public readonly ITableEntrySource source;
            public readonly int oldStatValue;
            public readonly int newStatValue;
            public readonly int totalDeltaValue;

            public PostSetArgs(int oldStatValue, int newStatValue, bool isAbsolute, ITableEntrySource source)
            {
                this.isAbsolute = isAbsolute;
                this.oldStatValue = oldStatValue;
                this.newStatValue = newStatValue;
                this.source = source;
                this.totalDeltaValue = newStatValue - oldStatValue;
            }
        }

        public TableStat(object owner, int absValue = 0)
        {
            _owner = owner;

            _onPreSet = new TableEventVoid<PreSetArgs>();
            _onPostSet = new TableEventVoid<PostSetArgs>();

            _absEntries = new TableEntryDict();
            _relEntries = new TableEntryDict();

            _absValue = absValue;
            _relValue = 1;
            _value = absValue;
        }
        private TableStat(TableStat src, TableStatCloneArgs args)
        {
            _owner = args.ownerClone;

            _onPreSet = (TableEventVoid<PreSetArgs>)src._onPreSet.Clone();
            _onPostSet = (TableEventVoid<PostSetArgs>)src._onPostSet.Clone();

            TableEntryDictCloneArgs entriesCArgs = new(args.terrCArgs);

            _absEntries = (TableEntryDict)src._absEntries.Clone(entriesCArgs);
            _relEntries = (TableEntryDict)src._relEntries.Clone(entriesCArgs);

            _absValue = src._absValue;
            _relValue = src._relValue;
            _value = src._value;
        }

        public static implicit operator int(TableStat stat) => stat._value;

        public void Dispose()
        {
            _onPreSet.Clear();
            _onPostSet.Clear();
            _absEntries.Clear();
            _relEntries.Clear();
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

        // use entryId and Revert methods to revert applied effect instead of calling these methods with negative value
        // (as value can be modified by OnPreSet event)

        public UniTask SetValueAbs(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value - _value, isAbsolute: true, source, entryId);
        }
        public UniTask SetValueRel(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value - _value, isAbsolute: false, source, entryId);
        }

        public UniTask AdjustValueAbs(float value, ITableEntrySource source, string entryId = null) 
        {
            return AdjustValue(value, isAbsolute: true, source, entryId);
        }
        public UniTask AdjustValueRel(float value, ITableEntrySource source, string entryId = null) 
        {
            return AdjustValue(value, isAbsolute: false, source, entryId);
        }

        public UniTask RevertValueAbs(string entryId)
        {
            return RevertValue(entryId, isAbsolute: true);
        }
        public UniTask RevertValueRel(string entryId)
        {
            return RevertValue(entryId, isAbsolute: false);
        }

        public float EntryValueAbs(string entryId)
        {
            if (_absEntries.TryGetValue(entryId, out TableEntry entry))
                 return entry.value;
            else return 0;
        }
        public float EntryValueRel(string entryId)
        {
            if (_relEntries.TryGetValue(entryId, out TableEntry entry))
                return entry.value;
            else return 0;
        }

        async UniTask AdjustValue(float value, bool isAbsolute, ITableEntrySource source, string entryId)
        {
            PreSetArgs preArgs = new(_value, value, isAbsolute, source);
            foreach (var preSetSub in _onPreSet)
            {
                await preSetSub.Delegate.Invoke(this, preArgs);
                if (preArgs.handled) return;
            }

            float preArgsDelta = preArgs.deltaValue;
            if (isAbsolute)
            {
                _absValue += preArgsDelta;
                if (preArgsDelta != 0)
                    _absEntries.Add(entryId, new TableEntry(preArgsDelta, source));
            }
            else
            {
                UpdateRelValue(preArgsDelta);
                if (preArgsDelta != 0)
                    _relEntries.Add(entryId, new TableEntry(preArgsDelta, source));
            }

            RecalculateValue();
            await _onPostSet.Invoke(this, new PostSetArgs(preArgs.oldStatValue, _value, isAbsolute, source));
        }
        async UniTask RevertValue(string entryId, bool isAbsolute)
        {
            int oldValue = _value;
            TableEntryDict entries = isAbsolute ? _absEntries : _relEntries;
            if (!entries.TryGetValue(entryId, out TableEntry entry))
                return;

            if (isAbsolute)
                 _absValue -= entry.value;
            else UpdateRelValue(-entry.value);

            entries.Remove(entryId);
            RecalculateValue();

            await _onPostSet.Invoke(this, new PostSetArgs(oldValue, _value, isAbsolute, null));
        }

        void UpdateRelValue(float delta)
        {
            _relDelta += delta;
            if (_relDelta > 0)
                 _relValue = 1 * (1 + _relDelta);
            else _relValue = 1 / (1 - _relDelta);
        }
        void RecalculateValue()
        {
            _value = (_absValue * _relValue).Ceiling();
        }
    }
}
