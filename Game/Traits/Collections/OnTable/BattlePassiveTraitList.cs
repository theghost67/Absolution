using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий список пассивных навыков во время сражения (см. <see cref="BattlePassiveTrait"/>).<br/>
    /// Список является частью набора списков навыков (см. <see cref="BattleTraitListSet"/>).
    /// </summary>
    public class BattlePassiveTraitList : TablePassiveTraitList, IBattleTraitList, IReadOnlyList<BattlePassiveTraitListElement>
    {
        public new BattleTraitListSet Set { get; }
        public BattlePassiveTraitList(BattleTraitListSet set) : base(set)
        {
            Set = set;
            TryOnInstantiatedAction(GetType(), typeof(BattlePassiveTraitList));
        }
        protected BattlePassiveTraitList(BattlePassiveTraitList src, BattleTraitListCloneArgs args) : base(src, args)
        {
            Set = args.srcListSetClone;
            TryOnInstantiatedAction(GetType(), typeof(BattlePassiveTraitList));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleTraitListCloneArgs cArgs)
                return new BattlePassiveTraitList(this, cArgs);
            else return null;
        }

        public new BattlePassiveTraitListElement this[string id] => (BattlePassiveTraitListElement)GetElement(id);
        public new BattlePassiveTraitListElement this[int index] => (BattlePassiveTraitListElement)GetElement(index);
        public new IEnumerator<BattlePassiveTraitListElement> GetEnumerator() => GetElements().Cast<BattlePassiveTraitListElement>().GetEnumerator();

        protected override ITableTraitListElement ElementCreator(TableTraitStacksTryArgs e)
        {
            BattlePassiveTraitListElement element = this[e.id];
            if (element != null)
            {
                element.AdjustStacksInternal(e.delta);
                return element;
            }

            BattlePassiveTrait trait = new(TraitBrowser.NewPassive(e.id), Set.Owner, null);
            element = new BattlePassiveTraitListElement(this, trait, e.delta);
            return element;
        }
        protected override ITableTraitListElement ElementRemover(TableTraitStacksTryArgs e)
        {
            BattlePassiveTraitListElement element = this[e.id];
            element?.AdjustStacksInternal(e.delta);
            return element;
        }
        protected override ITableTraitListElement ElementCloner(ITableTraitListElement src, TableTraitListCloneArgs args)
        {
            BattleTraitListCloneArgs argsCast = (BattleTraitListCloneArgs)args;
            BattleTraitListElementCloneArgs elementCArgs = new(this, argsCast.terrCArgs);
            return (BattlePassiveTraitListElement)src.Clone(elementCArgs);
        }

        IEnumerator<IBattleTraitListElement> IEnumerable<IBattleTraitListElement>.GetEnumerator() => GetEnumerator();
    }
}
