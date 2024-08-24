using Cysharp.Threading.Tasks;
using Game.Core;
using GreenOne;
using MyBox;
using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, представляющий характеристику типа <see cref="int"/>, изменение значения которой вызывает события перед/после изменения этой характеристики.<br/>
    /// Событие, вызываемое перед изменением, может полностью отменить это изменение.
    /// </summary>
    public sealed class TableStat : ITableLoggable, ICloneableWithArgs, IDisposable
    {
        public ITableEventVoid<PreSetArgs> OnPreSet => _onPreSet;
        public ITableEventVoid<PostSetArgs> OnPostSet => _onPostSet;

        public string Id => _id;
        public object Owner => _owner;

        public int Value => _value;
        public float ValueRaw => _valueRaw;
        public float ValueScale => _valueScale;

        public string TableName => _id;
        public string TableNameDebug => _id;

        readonly string _id;
        readonly object _owner;
        readonly TableEventVoid<PreSetArgs> _onPreSet;
        readonly TableEventVoid<PostSetArgs> _onPostSet;
        readonly TableEntryDict _valueEntries;
        readonly TableEntryDict _valueScaleEntries;

        int _value;
        float _valueRaw;
        float _valueAbsPositive;
        float _valueAbsNegative; // used only in health stat
        float _valueScale;
        float _valueScaleDelta;

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

            _valueAbsPositive = defaultValue;
            _valueAbsNegative = 0;
            _valueScale = 1;
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

            _valueAbsPositive = src._valueAbsPositive;
            _valueAbsNegative = src._valueAbsNegative;
            _valueScale = src._valueScale;
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
        public string ToStringRich(int defaultValue)
        {
            int value = _value;
            Color statColor;
            if (value > defaultValue)
                statColor = Color.green;
            else if (value < defaultValue)
                statColor = Color.red;
            else statColor = Color.white;

            if (_id == "health")
                 return $"{ToString().Colored(statColor)} ({_valueAbsPositive.Rounded(2)} * {(_valueScale * 100).Rounded(2)}% - {_valueAbsNegative.Rounded(2)})";
            else return $"{ToString().Colored(statColor)} ({_valueAbsPositive.Rounded(2)} * {(_valueScale * 100).Rounded(2)}%)";
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

        public UniTask SetValueScale(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value - _valueScale, source, entryId, true, false);
        }
        public UniTask SetValue(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value - (_valueAbsPositive - _valueAbsNegative), source, entryId, false, _id != "health");
        }

        public UniTask AdjustValueScale(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value, source, entryId, true, false);
        }
        public UniTask AdjustValue(float value, ITableEntrySource source, string entryId = null)
        {
            return AdjustValue(value, source, entryId, false, _id != "health");
        }

        public UniTask RevertValue(string entryId)
        {
            return RevertValue(entryId, false);
        }
        public UniTask RevertValueScale(string entryId)
        {
            return RevertValue(entryId, true);
        }

        async UniTask AdjustValue(float value, ITableEntrySource source, string entryId, bool isRelative, bool asDefault)
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
                if (asDefault)
                    _valueAbsPositive += preArgsDelta;
                else if (preArgsDelta < 0)
                     _valueAbsNegative -= preArgsDelta;
                else _valueAbsPositive += preArgsDelta;
                _valueEntries.Add(entryId, entry);
            }
            else
            {
                UpdateValueScale(preArgsDelta);
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
                    _valueAbsNegative += entry.value;
                else _valueAbsPositive -= entry.value;
            }
            else UpdateValueScale(-entry.value);

            entries.Remove(entryId);
            RecalculateValue();
            await _onPostSet.Invoke(this, new PostSetArgs(oldValue, _value, isRelative, null));
        }

        void UpdateValueScale(float delta)
        {
            _valueScaleDelta += delta;
            if (_valueScaleDelta > 0)
                 _valueScale = 1 * (1 + _valueScaleDelta);
            else _valueScale = 1 / (1 - _valueScaleDelta);
        }
        void RecalculateValue()
        {
            _valueRaw = _valueAbsPositive * _valueScale - _valueAbsNegative;
            _value = _valueRaw.Ceiling();
        }
    }
}
