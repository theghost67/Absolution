using Cysharp.Threading.Tasks;
using Game.Cards;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс для любого трейта, находящегося на столе.
    /// </summary>
    public abstract class TableTrait : TableObject, ITableTrait
    {
        public Trait Data => _data;
        public TableFieldCard Owner => _owner;
        public TableTraitStorage Storage => _storage;
        public new TableTraitDrawer Drawer => ((TableObject)this).Drawer as TableTraitDrawer;
        public virtual TableFinder Finder => null;

        public override string TableName => $"{Data.name}[{Owner?.Field?.TableName ?? "-"}]";
        public override string TableNameDebug => $"{Data.id}[{Owner?.Field?.TableNameDebug ?? "-"}]+{GuidStr}";

        readonly Trait _data;
        readonly TableFieldCard _owner;
        readonly TableTraitStorage _storage;

        public TableTrait(Trait data, TableFieldCard owner, Transform parent): base(parent)
        {
            _data = data;
            _owner = owner;
            _storage = new TableTraitStorage(this, _data.storage);

            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableTrait));
        }
        protected TableTrait(TableTrait src, TableTraitCloneArgs args) : base(src)
        {
            _data = args.srcTraitDataClone;
            _owner = args.srcTraitOwnerClone;
            _storage = new TableTraitStorage(this, src._storage);
            
            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableTrait));
        }

        public virtual void Dispose()
        {
            Drawer?.Dispose();
            _storage.Clear();
        }
        public abstract object Clone(CloneArgs args);

        public abstract int GetStacks();
        public abstract UniTask AdjustStacks(int delta, ITableEntrySource source);

        public UniTask SetStacks(int value, ITableEntrySource source)
        {
            return AdjustStacks(value - GetStacks(), source);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableTraitDrawer(this, parent);
        }
    }
}
