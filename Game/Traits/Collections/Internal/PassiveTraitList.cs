using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий список данных пассивных навыков (см. <see cref="PassiveTraitListElement"/>).
    /// </summary>
    public class PassiveTraitList : TraitList, IReadOnlyList<PassiveTraitListElement>
    {
        public PassiveTraitList(TraitListSet set) : base(set) { }
        protected PassiveTraitList(PassiveTraitList src, TraitListCloneArgs args) : base(src, args)
        {
            args.TryOnClonedAction(src.GetType(), typeof(PassiveTraitList));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TraitListCloneArgs cArgs)
                return new PassiveTraitList(this, cArgs);
            else return null;
        }

        public new PassiveTraitListElement this[string id] => (PassiveTraitListElement)GetElement(id);
        public new PassiveTraitListElement this[int index] => (PassiveTraitListElement)GetElement(index);
        public new IEnumerator<PassiveTraitListElement> GetEnumerator() => GetElements().Cast<PassiveTraitListElement>().GetEnumerator();

        protected override TraitListElement ElementCreator(TraitListEventArgs e)
        {
            PassiveTraitListElement element = this[e.id];
            if (element != null)
            {
                element.AdjustStacksInternal(e.stacks);
                return element;
            }

            PassiveTrait trait = TraitBrowser.NewPassive(e.id);
            element = new PassiveTraitListElement(this, trait);
            element.AdjustStacksInternal(e.stacks);
            return element;
        }
        protected override TraitListElement ElementRemover(TraitListEventArgs e)
        {
            PassiveTraitListElement element = this[e.id];
            element?.AdjustStacksInternal(e.stacks);
            return element;
        }
        protected override TraitListElement ElementCloner(TraitListElement src, TraitListCloneArgs args)
        {
            TraitListElementCloneArgs elementCArgs = new(this);
            return (PassiveTraitListElement)src.Clone(elementCArgs);
        }
    }
}
