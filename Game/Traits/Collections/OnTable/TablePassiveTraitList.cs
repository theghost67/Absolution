﻿using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий список пассивных навыков на столе (см. <see cref="TablePassiveTrait"/>).<br/>
    /// Список является частью набора списков навыков (см. <see cref="TableTraitListSet"/>).
    /// </summary>
    public class TablePassiveTraitList : TableTraitList, IReadOnlyList<TablePassiveTraitListElement>
    {
        public TablePassiveTraitList(TableTraitListSet set) : base(set) 
        {
            TryOnInstantiatedAction(GetType(), typeof(TablePassiveTraitList));
        }
        protected TablePassiveTraitList(TablePassiveTraitList src, TableTraitListCloneArgs args) : base(src, args) 
        {
            TryOnInstantiatedAction(GetType(), typeof(TablePassiveTraitList));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableTraitListCloneArgs cArgs)
                return new TablePassiveTraitList(this, cArgs);
            else return null;
        }

        public new TablePassiveTraitListElement this[string id] => (TablePassiveTraitListElement)GetElement(id);
        public new TablePassiveTraitListElement this[int index] => (TablePassiveTraitListElement)GetElement(index);
        public new IEnumerator<TablePassiveTraitListElement> GetEnumerator() => GetElements().Cast<TablePassiveTraitListElement>().GetEnumerator();

        protected override ITableTraitListElement ElementCreator(TableTraitStacksTryArgs e)
        {
            TablePassiveTraitListElement element = this[e.id];
            if (element != null)
            {
                element.AdjustStacksInternal(e.delta);
                return element;
            }

            TablePassiveTrait trait = new(Set.Owner.Data.traits.Passives[e.id]?.Trait ?? TraitBrowser.NewPassive(e.id), Set.Owner, null);
            element = new TablePassiveTraitListElement(this, trait, e.delta);
            return element;
        }
        protected override ITableTraitListElement ElementRemover(TableTraitStacksTryArgs e)
        {
            TablePassiveTraitListElement element = this[e.id];
            element?.AdjustStacksInternal(e.delta);
            return element;
        }
        protected override ITableTraitListElement ElementCloner(ITableTraitListElement src, TableTraitListCloneArgs args)
        {
            TableTraitListElementCloneArgs elementCArgs = new(this, args.terrCArgs);
            return (TablePassiveTraitListElement)src.Clone(elementCArgs);
        }
    }
}
