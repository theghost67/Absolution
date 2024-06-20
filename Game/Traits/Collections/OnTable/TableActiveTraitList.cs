using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий список активных трейтов на столе (см. <see cref="TableActiveTrait"/>).<br/>
    /// Список является частью набора списков трейтов (см. <see cref="TableTraitListSet"/>).
    /// </summary>
    public class TableActiveTraitList : TableTraitList, IReadOnlyList<TableActiveTraitListElement>
    {
        public TableActiveTraitList(TableTraitListSet set) : base(set) { }
        protected TableActiveTraitList(TableActiveTraitList src, TableTraitListCloneArgs args) : base(src, args)
        {
            args.TryOnClonedAction(src.GetType(), typeof(TableActiveTraitList));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableTraitListCloneArgs cArgs)
                return new TableActiveTraitList(this, cArgs);
            else return null;
        }

        public new TableActiveTraitListElement this[string id] => (TableActiveTraitListElement)GetElement(id);
        public new TableActiveTraitListElement this[int index] => (TableActiveTraitListElement)GetElement(index);
        public new IEnumerator<TableActiveTraitListElement> GetEnumerator() => GetElements().Cast<TableActiveTraitListElement>().GetEnumerator();

        protected override ITableTraitListElement ElementCreator(TableTraitStacksTryArgs e)
        {
            TableActiveTraitListElement element = this[e.id];
            if (element != null)
            {
                element.AdjustStacksInternal(e.stacks);
                return element;
            }

            TableActiveTrait trait = new(TraitBrowser.NewActive(e.id), Set.Owner, null, withDrawer: false);
            element = new TableActiveTraitListElement(this, trait, Set.Drawer != null);
            element.AdjustStacksInternal(e.stacks);
            return element;
        }
        protected override ITableTraitListElement ElementRemover(TableTraitStacksTryArgs e)
        {
            TableActiveTraitListElement element = this[e.id];
            element?.AdjustStacksInternal(e.stacks);
            return element;
        }
        protected override ITableTraitListElement ElementCloner(ITableTraitListElement src, TableTraitListCloneArgs args)
        {
            TableTraitListElementCloneArgs elementCArgs = new(this, args.terrCArgs);
            return (TableActiveTraitListElement)src.Clone(elementCArgs);
        }
    }
}
