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
        public new TablePassiveTraitListElementDrawer Drawer => _drawer;

        readonly TablePassiveTraitList _list;
        readonly TablePassiveTrait _trait;
        TablePassiveTraitListElementDrawer _drawer;

        public TablePassiveTraitListElement(TablePassiveTraitList list, TablePassiveTrait trait, bool withDrawer = true) : base(list, trait, withDrawer: false)
        {
            _list = list;
            _trait = trait;

            if (withDrawer)
                CreateDrawer(_list.Set.Drawer.transform);
        }
        protected TablePassiveTraitListElement(TablePassiveTraitListElement src, TableTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (TablePassiveTraitList)args.srcListClone;
            _trait = (TablePassiveTrait)base.Trait;
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
        protected override void DrawerSetter(TableTraitListElementDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (TablePassiveTraitListElementDrawer)value;
        }
        protected override TableTraitListElementDrawer DrawerCreator(Transform parent)
        {
             return new TablePassiveTraitListElementDrawer(this, parent);
        }
    }
}
