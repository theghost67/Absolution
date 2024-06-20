﻿using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка пассивных трейтов на столе (см. <see cref="BattlePassiveTraitList"/>).
    /// </summary>
    public class BattlePassiveTraitListElement : TablePassiveTraitListElement, IBattleTraitListElement
    {
        public new BattlePassiveTraitList List => _list;
        public new BattlePassiveTrait Trait => _trait;
        public new BattlePassiveTraitListElementDrawer Drawer => _drawer;

        readonly BattlePassiveTraitList _list;
        readonly BattlePassiveTrait _trait;
        BattlePassiveTraitListElementDrawer _drawer;

        IBattleTraitList IBattleTraitListElement.List => _list;
        IBattleTrait IBattleTraitListElement.Trait => _trait;

        public BattlePassiveTraitListElement(BattlePassiveTraitList list, BattlePassiveTrait trait, bool withDrawer = true) : base(list, trait, withDrawer: false)
        {
            _list = list;
            _trait = trait;

            if (withDrawer)
                CreateDrawer(_list.Set.Drawer.transform);
        }
        protected BattlePassiveTraitListElement(BattlePassiveTraitListElement src, BattleTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (BattlePassiveTraitList)args.srcListClone;
            _trait = (BattlePassiveTrait)base.Trait;
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleTraitListElementCloneArgs cArgs)
                return new BattlePassiveTraitListElement(this, cArgs);
            else return null;
        }

        protected override ITableTrait TraitCloner(ITableTrait src, TableTraitListElementCloneArgs args)
        {
            BattleTraitListElementCloneArgs argsCast = (BattleTraitListElementCloneArgs)args;
            BattlePassiveTraitCloneArgs traitCArgs = new((PassiveTrait)src.Data.Clone(), argsCast.srcListClone.Set.Owner, argsCast.terrCArgs);
            return (BattlePassiveTrait)src.Clone(traitCArgs);
        }
        protected override void DrawerSetter(TableTraitListElementDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (BattlePassiveTraitListElementDrawer)value;
        }
        protected override TableTraitListElementDrawer DrawerCreator(Transform parent)
        {
            return new BattlePassiveTraitListElementDrawer(this, parent);
        }
    }
}