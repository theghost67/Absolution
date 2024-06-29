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
        public new TableFieldCardDrawer Drawer => _drawer;
        public override TableFinder Finder => _finder;

        public override string TableName => $"{Data.name}[{_field?.TableName ?? "-"}]";
        public override string TableNameDebug => $"{Data.id}[{_field?.TableNameDebug ?? "-"}]+{GuidStr}";

        public readonly TableStat moxie;
        public readonly TableStat health;
        public readonly TableStat strength;

        public TableField Field => _field;
        public TableTraitListSet Traits => _traits;

        readonly FieldCard _data;
        readonly TableFieldCardFinder _finder;
        TableFieldCardDrawer _drawer;
        TableField _field;
        TableTraitListSet _traits;

        public TableFieldCard(FieldCard data, Transform parent, bool withDrawer = true, bool fillTraits = true) : base(data, parent, withDrawer: false)
        {
            _data = data;
            _finder = new TableFieldCardFinder(this);

            health = new TableStat(nameof(health), this, data.health);
            health.OnPreSet.Add(OnStatPreSetBase_TOP, 256);
            health.OnPostSet.Add(OnStatPostSetBase_TOP, 256);

            strength = new TableStat(nameof(strength), this, data.strength);
            strength.OnPreSet.Add(OnStatPreSetBase_TOP, 256);
            strength.OnPostSet.Add(OnStatPostSetBase_TOP, 256);

            moxie = new TableStat(nameof(moxie), this, data.moxie);
            moxie.OnPreSet.Add(OnStatPreSetBase_TOP, 256);
            moxie.OnPostSet.Add(OnStatPostSetBase_TOP, 256);

            _traits = TraitListSetCreator();

            if (fillTraits)
                _traits.AdjustStacksInRange(data.traits, this);

            if (withDrawer)
                CreateDrawer(parent);
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
            args.AddOnClonedAction(src.GetType(), typeof(TableFieldCard), () => TraitListSetSetter(TraitListSetCloner(src._traits, args)));
        }

        public override void Dispose()
        {
            base.Dispose();
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
            bool result = e.field != null && e.field.Card == null;
            return UniTask.FromResult(result);
        }
        public async UniTask AttachToAnotherField(TableField field, ITableEntrySource source)
        {
            TableEventManager.Add();
            TableFieldAttachArgs args = new(field, source);
            if (await CanBeAttachedToField(args))
                await FieldPropSetter(args);
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
        protected virtual void TraitListSetSetter(TableTraitListSet value)
        {
            _traits = value;
        }

        protected virtual async UniTask FieldPropSetter(TableFieldAttachArgs e)
        {
            if (e.field == null)
            {
                FieldBaseSetter(null);
                await _field.DetatchCard(e.source);
            }
            else
            {
                FieldBaseSetter(e.field);
                await _field.AttachCard(this, e.source);
            }
        }
        protected virtual void FieldBaseSetter(TableField value)
        {
            _field = value;
        }

        protected override void DrawerSetter(TableCardDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (TableFieldCardDrawer)value;
        }
        protected override TableCardDrawer DrawerCreator(Transform parent)
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
            owner.Traits.DestroyDrawer(true);
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
