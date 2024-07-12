using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка активных навыков на столе (см. <see cref="TableActiveTraitList"/>).
    /// </summary>
    public class TableActiveTraitListElement : TableTraitListElement
    {
        public new TableActiveTraitList List => _list;
        public new TableActiveTrait Trait => _trait;
        public new TableActiveTraitListElementDrawer Drawer => ((TableObject)this).Drawer as TableActiveTraitListElementDrawer;

        readonly TableActiveTraitList _list;
        readonly TableActiveTrait _trait;

        public TableActiveTraitListElement(TableActiveTraitList list, TableActiveTrait trait) : base(list, trait)
        {
            _list = list;
            _trait = trait;
            TryOnInstantiatedAction(GetType(), typeof(TableActiveTraitListElement));
        }
        protected TableActiveTraitListElement(TableActiveTraitListElement src, TableTraitListElementCloneArgs args) : base(src, args)
        {
            _list = (TableActiveTraitList)args.srcListClone;
            _trait = (TableActiveTrait)base.Trait;
            TryOnInstantiatedAction(GetType(), typeof(TableActiveTraitListElement));
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
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableActiveTraitListElementDrawer(this, parent);
        }
    }
}
