using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Menus;
using GreenOne;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровую территорию на столе из массива игровых полей (см. <see cref="TableField"/>).
    /// </summary>
    public class TableTerritory : TableObject, ITableLoggable, ICloneableWithArgs
    {
        public const int MAX_WIDTH = 5;
        public const int MAX_HEIGHT = 2;
        public const int MAX_SIZE = MAX_WIDTH * MAX_HEIGHT;

        public override string TableName => "Территория" + (_fieldsDrawersAreNull ? " (виртуальная)" : "");
        public override string TableNameDebug => $"{Menu.GetCurrent().Id}/territory+{GuidStr}";
        public bool DrawersAreNull => _fieldsDrawersAreNull;

        public ITableEventVoid<TableFieldAttachArgs> OnAnyCardAttachedToField => _onAnyCardAttachedToField;
        public ITableEventVoid<TableFieldAttachArgs> OnAnyCardDetatchedFromField => _onAnyCardDetatchedFromField;
        public int2 Grid => _grid;
        public Transform Transform => _transform;
        public Transform TransformForFields => _transformForFields;

        readonly TableEventVoid<TableFieldAttachArgs> _onAnyCardAttachedToField;
        readonly TableEventVoid<TableFieldAttachArgs> _onAnyCardDetatchedFromField;
        readonly int2 _grid;
        readonly AlignSettings _alignSettings;
        readonly TableField[,] _fields;

        Transform _transform;
        Transform _transformForFields;
        bool _fieldsDrawersAreNull;

        public TableTerritory(int2 grid, Transform parent, bool createFields = true) : base(parent)
        {
            if (grid.x > 5) 
                throw new ArgumentException("Territory size X cannot be larger than 5.");
            float2 alignDistance = new(TableFieldDrawer.WIDTH + 8, TableFieldDrawer.HEIGHT + 8);

            _onAnyCardAttachedToField = new TableEventVoid<TableFieldAttachArgs>();
            _onAnyCardDetatchedFromField = new TableEventVoid<TableFieldAttachArgs>();
            _grid = grid;
            _alignSettings = new AlignSettings(Vector2.zero, AlignAnchor.MiddleCenter, alignDistance, true, grid.x);
            _fields = new TableField[grid.x, grid.y];
            AddOnInstantiatedAction(GetType(), typeof(TableTerritory), () =>
            {
                if (createFields)
                    CreateFields();
            });
        }
        protected TableTerritory(TableTerritory src, TableTerritoryCloneArgs args) : base(src)
        {
            _onAnyCardAttachedToField = (TableEventVoid<TableFieldAttachArgs>)src._onAnyCardAttachedToField.Clone();
            _onAnyCardDetatchedFromField = (TableEventVoid<TableFieldAttachArgs>)src._onAnyCardDetatchedFromField.Clone();
            _grid = src.Grid;
            _fields = new TableField[Grid.x, Grid.y];
            _fieldsDrawersAreNull = true;
            _alignSettings = src._alignSettings;
            AddOnInstantiatedAction(GetType(), typeof(TableTerritory), () =>
            {
                CloneFields(src, args);
                args.OnTerritoryReadyInvoke(this);
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            _fieldsDrawersAreNull = true;
            foreach (TableField field in _fields)
                field?.Dispose();
            if (Transform != null)
                Transform.gameObject.Destroy();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableTerritoryCloneArgs cArgs)
                 return new TableTerritory(this, cArgs);
            else return null;
        }

        public void SetFieldsColliders(bool value)
        {
            foreach (TableField field in Fields())
                field.Drawer?.SetCollider(value);
        }
        public void SetFieldsHighlight(bool value)
        {
            foreach (TableField field in Fields())
                field.Drawer?.SetHighlight(value);
        }
        public void SetCardsColliders(bool value)
        {
            foreach (TableField field in Fields().WithCard())
                field.Card.Drawer?.SetCollider(value);
        }

        public void CreateFields()
        {
            for (int y = 0; y < Grid.y; y++)
            {
                for (int x = 0; x < Grid.x; x++)
                {
                    TableField field = FieldCreator(x, y);
                    if (field == null) continue;

                    field.OnCardAttached.Add(GuidStr, OnAnyCardAttachedToFieldBase_TOP, TableEventVoid.TOP_PRIORITY);
                    field.OnCardDetatched.Add(GuidStr, OnAnyCardDetatchedFromFieldBase_TOP, TableEventVoid.TOP_PRIORITY);

                    _fields[x, y]?.Dispose();
                    FieldSetter(x, y, field);
                }
            }
        }
        public void DestroyFields()
        {
            for (int y = 0; y < Grid.y; y++)
            {
                for (int x = 0; x < Grid.x; x++)
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
            if (x < 0 || x >= Grid.x) return null;
            if (y < 0 || y >= Grid.y) return null;
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
            for (int y = 0; y < Grid.y; y++)
            {
                for (int x = 0; x < Grid.x; x++)
                {
                    if (_fields[x, y] != null)
                        yield return _fields[x, y];
                }
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

            bool FilterBase(int2 pos) => pos.x < Grid.x && pos.y < Grid.y && _fields[pos.x, pos.y] != null;
            Predicate<int2> filter;

            bool exludeSelf = range.targets.HasFlag(TerritoryTargets.NotSelf);
            if (exludeSelf)
                 filter = pos => FilterBase(pos) && !pos.Equals(centerPos);
            else filter = FilterBase;

            foreach (int2 pos in range.Overlap(centerPos, filter))
                yield return _fields[pos.x, pos.y];
        }

        // use to invoke handler on all currently placed cards and all cards that will be attached later
        // if (source == null) handler was invoked on a already placed card
        public void ContinuousAttachHandler_Add(string guid, IdEventVoidHandlerAsync<TableFieldAttachArgs> handler)
        {
            foreach (TableField field in Fields().WithCard())
                handler.Invoke(this, new TableFieldAttachArgs(field.Card, field, null));
            _onAnyCardAttachedToField.Add(guid, handler);
        }
        public void ContinuousAttachHandler_Remove(string guid, IdEventVoidHandlerAsync<TableFieldAttachArgs> handler)
        {
            foreach (TableField field in Fields().WithCard())
                handler.Invoke(this, new TableFieldAttachArgs(field.Card, field, null));
            _onAnyCardAttachedToField.Remove(guid);
        }

        protected virtual UniTask OnAnyCardAttachedToFieldBase_TOP(object sender, TableFieldAttachArgs e)
        {
            TableTerritory terr = e.field.Territory;
            return terr._onAnyCardAttachedToField.Invoke(terr, e);
        }
        protected virtual UniTask OnAnyCardDetatchedFromFieldBase_TOP(object sender, TableFieldAttachArgs e)
        {
            TableTerritory terr = e.field.Territory;
            return terr._onAnyCardDetatchedFromField.Invoke(terr, e);
        }

        protected virtual TableField FieldCreator(int x, int y)
        {
            return new TableField(this, new int2(x, y), TransformForFields);
        }
        protected virtual TableField FieldCloner(TableField src, TableTerritoryCloneArgs args)
        {
            TableFieldCloneArgs fieldCArgs = new(this, args);
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
            for (int y = 0; y < Grid.y; y++)
            {
                for (int x = 0; x < Grid.x; x++)
                {
                    TableField srcField = src._fields[x, y];
                    if (srcField != null)
                        FieldSetter(x, y, FieldCloner(srcField, args));
                }
            }
        }

        protected override void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            base.OnDrawerCreatedBase(sender, e);

            _fieldsDrawersAreNull = false;
            _transform = Drawer.transform;
            _transformForFields = _transform.CreateEmptyObject("Fields");

            foreach (TableField field in Fields())
                field.CreateDrawer(_transformForFields);

            _alignSettings.ApplyTo(_transformForFields);
        }
        protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            base.OnDrawerDestroyedBase(sender, e);

            _fieldsDrawersAreNull = true;
            _transform = null;
            _transformForFields = null;

            foreach (TableField field in Fields())
                field.DestroyDrawer(Drawer?.IsDestroyed ?? true);
        }
        protected override Drawer DrawerCreator(Transform parent) => new DrawerShell(this, parent.CreateEmptyObject("Territory"));
    }
}
