using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка пассивных трейтов на столе (см. <see cref="TablePassiveTraitList"/>).
    /// </summary>
    public class TablePassiveTraitListElement : TableTraitListElement
    {
        public new TablePassiveTraitList List => _list;
        public new TablePassiveTrait Trait => _trait;
        public new TablePassiveTraitListElementDrawer Drawer => ((TableObject)this).Drawer as TablePassiveTraitListElementDrawer;

        readonly TablePassiveTraitList _list;
        readonly TablePassiveTrait _trait;

        public TablePassiveTraitListElement(TablePassiveTraitList list, TablePassiveTrait trait) : base(list, trait)
        {
            _list = list;
            _trait = trait;
            TryOnInstantiatedAction(GetType(), typeof(TablePassiveTraitListElement));
        }
        protected TablePassiveTraitListElement(TablePassiveTraitListElement src, TableTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (TablePassiveTraitList)args.srcListClone;
            _trait = (TablePassiveTrait)base.Trait;
            TryOnInstantiatedAction(GetType(), typeof(TablePassiveTraitListElement));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableTraitListElementCloneArgs cArgs)
                 return new TablePassiveTraitListElement(this, cArgs);
            else return null;
        }
        protected override ITableTrait TraitCloner(ITableTrait src, TableTraitListElementCloneArgs args)
        {
            TablePassiveTraitCloneArgs traitCArgs = new((PassiveTrait)src.Data.Clone(), args.srcListClone.Set.Owner, args.terrCArgs);
            return (TablePassiveTrait)src.Clone(traitCArgs);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
             return new TablePassiveTraitListElementDrawer(this, parent);
        }
    }
}
