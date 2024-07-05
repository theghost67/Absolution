using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий список данных активных трейтов (см. <see cref="ActiveTraitListElement"/>).
    /// </summary>
    public class ActiveTraitList : TraitList, IReadOnlyList<ActiveTraitListElement>
    {
        public ActiveTraitList(TraitListSet set) : base(set) 
        { 

        }
        protected ActiveTraitList(ActiveTraitList src, TraitListCloneArgs args) : base(src, args)
        {
            args.TryOnClonedAction(src.GetType(), typeof(ActiveTraitList));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TraitListCloneArgs cArgs)
                return new ActiveTraitList(this, cArgs);
            else return null;
        }

        public new ActiveTraitListElement this[string id] => (ActiveTraitListElement)GetElement(id);
        public new ActiveTraitListElement this[int index] => (ActiveTraitListElement)GetElement(index);
        public new IEnumerator<ActiveTraitListElement> GetEnumerator() => GetElements().Cast<ActiveTraitListElement>().GetEnumerator();

        protected override TraitListElement ElementCreator(TraitListEventArgs e)
        {
            ActiveTraitListElement element = this[e.id];
            if (element != null)
            {
                element.AdjustStacksInternal(e.stacks);
                return element;
            }

            ActiveTrait trait = TraitBrowser.NewActive(e.id);
            element = new ActiveTraitListElement(this, trait);
            element.AdjustStacksInternal(e.stacks);
            return element;
        }
        protected override TraitListElement ElementRemover(TraitListEventArgs e)
        {
            ActiveTraitListElement element = this[e.id];
            element?.AdjustStacksInternal(e.stacks);
            return element;
        }
        protected override TraitListElement ElementCloner(TraitListElement src, TraitListCloneArgs args)
        {
            TraitListElementCloneArgs elementCArgs = new(this);
            return (ActiveTraitListElement)src.Clone(elementCArgs);
        }
    }
}
