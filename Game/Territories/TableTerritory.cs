using Game.Cards;
using Game.Territories;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровую территорию на столе из массива игровых полей (см. <see cref="TableField"/>).
    /// </summary>
    public class TableTerritory : ITableDrawable, ICloneableWithArgs, IDisposable
    {
        public const int MAX_WIDTH = 5;
        public const int MAX_HEIGHT = 2;
        public const int MAX_SIZE = MAX_WIDTH * MAX_HEIGHT;
        public static readonly int2 maxGrid = new(MAX_WIDTH, MAX_HEIGHT);

        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public string Log => _logString;
        public Drawer Drawer => null;
        public bool HasFieldDrawers => _hasFieldDrawers;

        public readonly int2 grid;
        public readonly Transform transform;
        public readonly Transform transformForFields;

        readonly TableField[,] _fields;
        readonly AlignSettings _alignSettings;
        StringBuilder _logBuilder;
        string _logString;
        bool _hasFieldDrawers;

        public TableTerritory(int2 grid, Transform parent, bool createFields = true, bool withDrawers = true)
        {
            if (grid.x > 5) 
                throw new ArgumentException("Territory size X cannot be larger than 5.");

            parent = parent != null ? parent : Global.Root;
            float2 alignDistance = new(TableFieldDrawer.WIDTH + 8, TableFieldDrawer.HEIGHT + 8);

            this.grid = grid;
            transform = parent.CreateEmptyObject("Territory");
            transformForFields = transform.CreateEmptyObject("Fields");

            _fields = new TableField[grid.x, grid.y];
            _hasFieldDrawers = withDrawers;
            _alignSettings = new AlignSettings(Vector2.zero, AlignAnchor.MiddleCenter, alignDistance, true, grid.x);
            _logBuilder = new StringBuilder();

            if (createFields) 
                CreateFields();
        }
        protected TableTerritory(TableTerritory src, TableTerritoryCloneArgs args)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            grid = src.grid;

            _fields = new TableField[grid.x, grid.y];
            _hasFieldDrawers = false;
            _alignSettings = src._alignSettings;
            _logBuilder = new StringBuilder(src._logBuilder.ToString());
            _logBuilder.AppendLine("/// КЛОН ///");

            args.AddOnClonedAction(src.GetType(), typeof(TableTerritory), () =>
            {
                CloneFields(src, args);
                args.OnTerritoryReadyInvoke(this);
            });
        }

        public virtual void Dispose()
        {
            _hasFieldDrawers = false;

            foreach (TableField field in _fields)
                field?.Dispose();

            if (transform != null)
                transform.gameObject.Destroy();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableTerritoryCloneArgs cArgs)
                 return new TableTerritory(this, cArgs);
            else return null;
        }

        public virtual void WriteLog(string text, bool isForDebug = false)
        {
            _logBuilder.AppendLine(text);
            _logString = _logBuilder.ToString();
        }
        public void WriteLogForDebug(string text)
        {
            WriteLog(text, isForDebug: true);
        }

        public void CreateDrawer(Transform parent)
        {
            _hasFieldDrawers = true;
            foreach (TableField field in Fields())
                field.CreateDrawer(transform);
        }
        public void DestroyDrawer(bool instantly)
        {
            _hasFieldDrawers = false;
            foreach (TableField field in Fields())
                field.DestroyDrawer(instantly);
        }

        public void SetColliders(bool value)
        {
            foreach (TableField field in Fields())
            {
                if (!field.Drawer.IsDestroyed)
                    field.Drawer.SetCollider(value);

                if (field.Card != null && !field.Card.Drawer.IsDestroyed)
                    field.Card.Drawer.SetCollider(value);
            }
        }
        public void SetFieldsColliders(bool value)
        {
            foreach (TableField field in Fields())
            {
                if (!field.Drawer.IsDestroyed)
                    field.Drawer.SetCollider(value);
            }
        }
        public void SetFieldsHighlight(bool value)
        {
            foreach (TableField field in Fields())
            {
                if (!field.Drawer.IsDestroyed)
                    field.Drawer.SetHighlight(value);
            }
        }
        public void SetCardsColliders(bool value)
        {
            foreach (TableField field in Fields().WithCard())
                field.Card.Drawer?.SetCollider(value);
        }

        public void CreateFields()
        {
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    TableField field = FieldCreator(x, y);
                    if (field == null) continue;

                    _fields[x, y]?.Dispose();
                    FieldSetter(x, y, field);
                }
            }

            _alignSettings.ApplyTo(transformForFields);
        }
        public void DestroyFields()
        {
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    TableField field = _fields[x, y];

                    if (field == null) continue;
                    if (!FieldDestroyFilter(x, y)) continue;

                    field.Dispose();
                    FieldSetter(x, y, null);
                }
            }
        }

        public TableField Field(in int2 pos) 
        {
            return Field(pos.x, pos.y);
        }
        public TableField Field(in int x, in int y)
        {
            if (x < 0 || x >= grid.x) return null;
            if (y < 0 || y >= grid.y) return null;
            return _fields[x, y];
        }

        public TableField FieldOpposite(in int2 pos)
        {
            return FieldOpposite(pos.x, pos.y);
        }
        public TableField FieldOpposite(in int x, in int y)
        {
            return _fields[x, y == 0 ? 1 : 0];
        }

        public IEnumerable<TableField> Fields()
        {
            foreach (TableField field in _fields)
            {
                if (field != null)
                    yield return field;
            }
        }
        public IEnumerable<TableField> Fields(int2 centerPos, TerritoryRange range)
        {
            if (range == TerritoryRange.none)
                yield break;
            if (range == TerritoryRange.all)
            {
                foreach (TableField field in _fields)
                    yield return field;

                yield break;
            }
            if (range == TerritoryRange.ownerSingle)
            {
                yield return _fields[centerPos.x, centerPos.y];
                yield break;
            }

            bool FilterBase(int2 pos) => pos.x < grid.x && pos.y < grid.y && _fields[pos.x, pos.y] != null;
            Predicate<int2> filter;

            bool exludeSelf = range.targets.HasFlag(TerritoryTargets.NotSelf);
            if (exludeSelf)
                 filter = pos => FilterBase(pos) && !pos.Equals(centerPos);
            else filter = FilterBase;

            foreach (int2 pos in range.Overlap(centerPos, filter))
                yield return _fields[pos.x, pos.y];
        }

        protected virtual TableField FieldCreator(int x, int y)
        {
            return new TableField(this, new int2(x, y), transformForFields, HasFieldDrawers);
        }
        protected virtual TableField FieldCloner(TableField src, TableTerritoryCloneArgs args)
        {
            FieldCard srcFieldCardDataClone = (FieldCard)src.Card.Data.Clone();
            TableFieldCloneArgs fieldCArgs = new(srcFieldCardDataClone, args);
            return (TableField)src.Clone(fieldCArgs);
        }

        protected virtual bool FieldDestroyFilter(int x, int y)
        {
            return true;
        }
        protected virtual void FieldSetter(int x, int y, TableField value)
        {
            _fields[x, y] = value;
        }
        protected void CloneFields(TableTerritory src, TableTerritoryCloneArgs args)
        {
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    TableField srcField = src._fields[x, y];
                    if (srcField != null)
                        FieldSetter(x, y, FieldCloner(srcField, args));
                }
            }
        }
    }
}
