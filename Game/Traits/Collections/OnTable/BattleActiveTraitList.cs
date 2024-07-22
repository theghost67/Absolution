using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий список активных навыков во время сражения (см. <see cref="BattleActiveTrait"/>).<br/>
    /// Список является частью набора списков навыков (см. <see cref="BattleTraitListSet"/>).
    /// </summary>
    public class BattleActiveTraitList : TableActiveTraitList, IBattleTraitList, IReadOnlyList<BattleActiveTraitListElement>
    {
        public new BattleTraitListSet Set { get; }
        public BattleActiveTraitList(BattleTraitListSet set) : base(set) 
        {
            Set = set;
            TryOnInstantiatedAction(GetType(), typeof(BattleActiveTraitList));
        }
        protected BattleActiveTraitList(BattleActiveTraitList src, BattleTraitListCloneArgs args) : base(src, args) 
        {
            Set = args.srcListSetClone;
            TryOnInstantiatedAction(GetType(), typeof(BattleActiveTraitList));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleTraitListCloneArgs cArgs)
                return new BattleActiveTraitList(this, cArgs);
            else return null;
        }

        public new BattleActiveTraitListElement this[string id] => (BattleActiveTraitListElement)GetElement(id);
        public new BattleActiveTraitListElement this[int index] => (BattleActiveTraitListElement)GetElement(index);
        public new IEnumerator<BattleActiveTraitListElement> GetEnumerator() => GetElements().Cast<BattleActiveTraitListElement>().GetEnumerator();

        protected override ITableTraitListElement ElementCreator(TableTraitStacksTryArgs e)
        {
            BattleActiveTraitListElement element = this[e.id];
            if (element != null)
            {
                element.AdjustStacksInternal(e.delta);
                return element;
            }

            BattleActiveTrait trait = new(TraitBrowser.NewActive(e.id), Set.Owner, null);
            element = new BattleActiveTraitListElement(this, trait, e.delta);
            return element;
        }
        protected override ITableTraitListElement ElementRemover(TableTraitStacksTryArgs e)
        {
            BattleActiveTraitListElement element = this[e.id];
            element?.AdjustStacksInternal(e.delta);
            return element;
        }
        protected override ITableTraitListElement ElementCloner(ITableTraitListElement src, TableTraitListCloneArgs args)
        {
            BattleTraitListCloneArgs argsCast = (BattleTraitListCloneArgs)args;
            BattleTraitListElementCloneArgs elementCArgs = new(this, argsCast.terrCArgs);
            return (BattleActiveTraitListElement)src.Clone(elementCArgs);
        }

        IEnumerator<IBattleTraitListElement> IEnumerable<IBattleTraitListElement>.GetEnumerator() => GetEnumerator();
    }
}
