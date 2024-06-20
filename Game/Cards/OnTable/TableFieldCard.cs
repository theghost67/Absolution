using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Territories;
using Game.Traits;
using GreenOne;
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
        public override string TableName => $"{base.TableName}[{_field.PosToStringRich()}]";

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

            health = new TableStat(this, data.health);
            strength = new TableStat(this, data.strength);
            moxie = new TableStat(this, data.moxie);

            health.OnPreSet.Add(OnHealthPreSetBase_TOP, 256);
            strength.OnPreSet.Add(OnStrengthPreSetBase_TOP, 256);
            moxie.OnPreSet.Add(OnMoxiePreSetBase_TOP, 256);

            health.OnPostSet.Add(OnHealthPostSetBase_TOP, 256);
            strength.OnPostSet.Add(OnStrengthPostSetBase_TOP, 256);
            moxie.OnPostSet.Add(OnMoxiePostSetBase_TOP, 256);

            _traits = TraitListSetCreator();

            if (fillTraits)
                _traits.AdjustRange(data.traits, this);

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

        public virtual UniTask<bool> CanBeAttachedToField(TableField field)
        {
            return UniTask.FromResult(true);
        }
        public virtual async UniTask AttachToAnotherField(TableField field)
        {
            TableEventManager.Add();
            await FieldPropSetter(field);
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

        protected virtual async UniTask FieldPropSetter(TableField value)
        {
            if (value != null && value.Card != null) return;
            if (!await CanBeAttachedToField(value)) return;
            if (value == null)
            {
                FieldBaseSetter(null);
                await _field.DetatchCard();
            }
            else
            {
                FieldBaseSetter(value);
                await _field.AttachCard(this);
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

        // used in BattleFieldCard for logging
        protected virtual UniTask OnHealthPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnStrengthPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnMoxiePreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnHealthPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer.CreateDamageTextSplash(e.totalDeltaValue);
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnStrengthPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnMoxiePostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        // ----
    }
}
