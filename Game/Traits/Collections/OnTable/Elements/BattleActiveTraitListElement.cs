using Game.Cards;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка активных трейтов во время сражения (см. <see cref="BattleActiveTraitList"/>).
    /// </summary>
    public class BattleActiveTraitListElement : TableActiveTraitListElement, IBattleTraitListElement
    {
        public new BattleActiveTraitList List => _list;
        public new BattleActiveTrait Trait => _trait;
        public new BattleActiveTraitListElementDrawer Drawer => _drawer;

        readonly BattleActiveTraitList _list;
        readonly BattleActiveTrait _trait;
        BattleActiveTraitListElementDrawer _drawer;

        IBattleTraitList IBattleTraitListElement.List => _list;
        IBattleTrait IBattleTraitListElement.Trait => _trait;

        public BattleActiveTraitListElement(BattleActiveTraitList list, BattleActiveTrait trait, bool withDrawer = true) : base(list, trait, withDrawer: false)
        {
            _list = list;
            _trait = trait;

            if (withDrawer)
                CreateDrawer(_list.Set.Drawer.transform);
        }
        protected BattleActiveTraitListElement(BattleActiveTraitListElement src, BattleTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (BattleActiveTraitList)args.srcListClone;
            _trait = (BattleActiveTrait)base.Trait;
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleTraitListElementCloneArgs cArgs)
                return new BattleActiveTraitListElement(this, cArgs);
            else return null;
        }

        protected override ITableTrait TraitCloner(ITableTrait src, TableTraitListElementCloneArgs args)
        {
            BattleTraitListElementCloneArgs argsCast = (BattleTraitListElementCloneArgs)args;
            BattleActiveTraitCloneArgs traitCArgs = new((ActiveTrait)src.Data.Clone(), argsCast.srcListClone.Set.Owner, argsCast.terrCArgs);
            return (BattleActiveTrait)src.Clone(traitCArgs);
        }
        protected override void DrawerSetter(TableTraitListElementDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (BattleActiveTraitListElementDrawer)value;
        }
        protected override TableTraitListElementDrawer DrawerCreator(Transform parent)
        {
            return new BattleActiveTraitListElementDrawer(this, parent);;
        }
    }
}
