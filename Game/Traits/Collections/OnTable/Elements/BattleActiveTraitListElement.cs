using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка активных навыков во время сражения (см. <see cref="BattleActiveTraitList"/>).
    /// </summary>
    public class BattleActiveTraitListElement : TableActiveTraitListElement, IBattleTraitListElement
    {
        public new BattleActiveTraitList List => _list;
        public new BattleActiveTrait Trait => _trait;
        public new BattleActiveTraitListElementDrawer Drawer => ((TableObject)this).Drawer as BattleActiveTraitListElementDrawer;

        readonly BattleActiveTraitList _list;
        readonly BattleActiveTrait _trait;

        IBattleTraitList IBattleTraitListElement.List => _list;
        IBattleTrait IBattleTraitListElement.Trait => _trait;

        public BattleActiveTraitListElement(BattleActiveTraitList list, BattleActiveTrait trait, int stacks) : base(list, trait, stacks)
        {
            _list = list;
            _trait = trait;
            TryOnInstantiatedAction(GetType(), typeof(BattleActiveTraitListElement));
        }
        protected BattleActiveTraitListElement(BattleActiveTraitListElement src, BattleTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (BattleActiveTraitList)args.srcListClone;
            _trait = (BattleActiveTrait)base.Trait;
            TryOnInstantiatedAction(GetType(), typeof(BattleActiveTraitListElement));
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
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new BattleActiveTraitListElementDrawer(this, parent);;
        }
    }
}
