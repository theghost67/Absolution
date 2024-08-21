using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий любую карту поля на столе (см. <see cref="FieldCard"/>), имеющую собственные изменяемые характеристики.
    /// </summary>
    public class TableFieldCard : TableCard
    {
        public new FieldCard Data => _data;
        public new TableFieldCardDrawer Drawer => ((TableObject)this).Drawer as TableFieldCardDrawer;
        public override TableFinder Finder => _finder;

        public override string TableName => $"{Data.name}[{_field?.TableName ?? "-"}]";
        public override string TableNameDebug => $"{Data.id}[{_field?.TableNameDebug ?? "-"}]+{GuidStr}";

        public TableStat Moxie => _moxie;
        public TableStat Health => _health;
        public TableStat Strength => _strength;

        public TableField Field => _field;
        public TableField LastField => _lastField;
        public TableTraitListSet Traits => _traits;
        public int FieldsAttachments => _fieldsAttachments;

        readonly FieldCard _data;
        readonly TableFieldCardFinder _finder;
        readonly string _eventsGuid;

        readonly TableStat _moxie;
        readonly TableStat _health;
        readonly TableStat _strength;

        TableField _field;
        TableField _lastField;
        TableTraitListSet _traits;
        int _fieldsAttachments;

        public TableFieldCard(FieldCard data, Transform parent) : base(data, parent)
        {
            _data = data;
            _finder = new TableFieldCardFinder(this);
            _eventsGuid = this.GuidGen(2);

            _health = new TableStat("health", this, data.health);
            _health.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _health.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            _strength = new TableStat("strength", this, data.strength);
            _strength.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _strength.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            _moxie = new TableStat("moxie", this, data.moxie);
            _moxie.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _moxie.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            AddOnInstantiatedAction(GetType(), typeof(TableFieldCard), () =>
            {
                _traits = TraitListSetCreator();
                _traits.AdjustStacksInRange(data.traits, this);
            });
        }
        protected TableFieldCard(TableFieldCard src, TableFieldCardCloneArgs args) : base(src, args)
        {
            _data = args.srcCardDataClone;
            _finder = new TableFieldCardFinder(this);

            TableStatCloneArgs statCArgs = new(this, args.terrCArgs);
            _health = (TableStat)src._health.Clone(statCArgs);
            _strength = (TableStat)src._strength.Clone(statCArgs);
            _moxie = (TableStat)src._moxie.Clone(statCArgs);

            _field = args.srcCardFieldClone;
            AddOnInstantiatedAction(GetType(), typeof(TableFieldCard), () =>
            {
                _traits = TraitListSetCloner(src._traits, args);
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            _traits.Dispose();

            _health.OnPreSet.Clear();
            _health.OnPostSet.Clear();
            _strength.OnPreSet.Clear();
            _strength.OnPostSet.Clear();
            _moxie.OnPreSet.Clear();
            _moxie.OnPostSet.Clear();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is TableFieldCardCloneArgs cArgs)
                 return new TableFieldCard(this, cArgs);
            else return null;
        }

        public virtual UniTask<bool> CanBeAttachedToField(TableFieldAttachArgs e)
        {
            bool result = e.field != _field && (e.field == null || e.field.Card == null);
            return UniTask.FromResult(result);
        }
        public async UniTask TryAttachToField(TableField field, ITableEntrySource source)
        {
            TableFieldAttachArgs args = new(this, field, source);
            if (await CanBeAttachedToField(args))
                await AttachToFieldInternal(args);
        }

        protected virtual TableTraitListSet TraitListSetCreator()
        {
            return new TableTraitListSet(this);
        }
        protected virtual TableTraitListSet TraitListSetCloner(TableTraitListSet src, TableFieldCardCloneArgs args)
        {
            TableTraitListSetCloneArgs setArgs = new(this, args.terrCArgs);
            return (TableTraitListSet)src.Clone(setArgs);
        }

        protected virtual async UniTask AttachToFieldInternal(TableFieldAttachArgs e)
        {
            if (e.field == null)
            {
                if (_field == null) return;
                await _field.DetatchCard(e.source);
                _field = null;
            }
            else
            {
                if (_field != null) 
                    await Field.DetatchCard(e.source);
                _field = e.field;
                _lastField = _field;
                _fieldsAttachments++;
                await _field.AttachCard(this, e.source);
            }
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableFieldCardDrawer(this, parent) { SortingOrderDefault = 10 };
        }

        protected override void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            TableFieldCard owner = (TableFieldCard)sender;
            owner._traits.CreateDrawer(owner.Drawer.transform);
        }
        protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            TableFieldCard owner = (TableFieldCard)sender;
            owner._traits.DestroyDrawer(owner.Drawer?.IsDestroyed ?? true);
        }
    }
}
