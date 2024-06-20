using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка активных трейтов на столе (см. <see cref="TableActiveTraitList"/>).
    /// </summary>
    public class TableActiveTraitListElement : TableTraitListElement
    {
        public new TableActiveTraitList List => _list;
        public new TableActiveTrait Trait => _trait;
        public new TableActiveTraitListElementDrawer Drawer => _drawer;

        readonly TableActiveTraitList _list;
        readonly TableActiveTrait _trait;
        TableActiveTraitListElementDrawer _drawer;

        public TableActiveTraitListElement(TableActiveTraitList list, TableActiveTrait trait, bool withDrawer = true) : base(list, trait, withDrawer: false)
        {
            _list = list;
            _trait = trait;

            if (withDrawer)
                CreateDrawer(_list.Set.Drawer.transform);
        }
        protected TableActiveTraitListElement(TableActiveTraitListElement src, TableTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (TableActiveTraitList)args.srcListClone;
            _trait = (TableActiveTrait)base.Trait;
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableTraitListElementCloneArgs cArgs)
                return new TableActiveTraitListElement(this, cArgs);
            else return null;
        }

        protected override ITableTrait TraitCloner(ITableTrait src, TableTraitListElementCloneArgs args)
        {
            TableActiveTraitCloneArgs traitCArgs = new((ActiveTrait)src.Data.Clone(), args.srcListClone.Set.Owner, args.terrCArgs);
            return (TableActiveTrait)src.Clone(traitCArgs);
        }
        protected override void DrawerSetter(TableTraitListElementDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (TableActiveTraitListElementDrawer)value;
        }
        protected override TableTraitListElementDrawer DrawerCreator(Transform parent)
        {
            return new TableActiveTraitListElementDrawer(this, parent);
        }
    }
}
