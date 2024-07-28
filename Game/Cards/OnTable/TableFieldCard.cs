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

        public readonly TableStat moxie;
        public readonly TableStat health;
        public readonly TableStat strength;

        public TableField Field => _field;
        public TableField LastField => _lastField;
        public TableTraitListSet Traits => _traits;

        readonly FieldCard _data;
        readonly TableFieldCardFinder _finder;
        readonly string _eventsGuid;
        TableField _field;
        TableField _lastField;
        TableTraitListSet _traits;

        public TableFieldCard(FieldCard data, Transform parent) : base(data, parent)
        {
            _data = data;
            _finder = new TableFieldCardFinder(this);
            _eventsGuid = this.GuidGen(2);

            health = new TableStat(nameof(health), this, data.health);
            health.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            health.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            strength = new TableStat(nameof(strength), this, data.strength);
            strength.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            strength.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            moxie = new TableStat(nameof(moxie), this, data.moxie);
            moxie.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            moxie.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

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
            health = (TableStat)src.health.Clone(statCArgs);
            strength = (TableStat)src.strength.Clone(statCArgs);
            moxie = (TableStat)src.moxie.Clone(statCArgs);

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

            health.OnPreSet.Clear();
            health.OnPostSet.Clear();
            strength.OnPreSet.Clear();
            strength.OnPostSet.Clear();
            moxie.OnPreSet.Clear();
            moxie.OnPostSet.Clear();
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
            TableEventManager.Add();
            TableFieldAttachArgs args = new(field, source);
            if (await CanBeAttachedToField(args))
                await AttachToFieldInternal(args);
            TableEventManager.Remove();
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
                await _field.AttachCard(this, e.source);
            }
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            TableFieldCardDrawer drawer = new(this, parent);
            drawer.SetSortingOrder(10, asDefault: true);
            return drawer;
        }

        protected override void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            TableFieldCard owner = (TableFieldCard)sender;
            owner.Traits.CreateDrawer(owner.Drawer.transform);
        }
        protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            TableFieldCard owner = (TableFieldCard)sender;
            owner.Traits.DestroyDrawer(Drawer?.IsDestroyed ?? true);
        }

        // used in BattleFieldCard for debug-logging
        protected virtual UniTask OnStatPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnStatPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        // ----
    }
}
