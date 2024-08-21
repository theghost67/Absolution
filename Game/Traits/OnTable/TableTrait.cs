using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс для любого навыка, находящегося на столе.
    /// </summary>
    public abstract class TableTrait : TableObject, ITableTrait
    {
        public TableFieldCard Owner => _owner;
        public TableTerritory Territory => _owner.Field.Territory;
        public TableField Field => _owner.Field;

        public Trait Data => _data;
        public TableTraitStorage Storage => _storage;

        public int TurnAge { get; set; }
        public int TurnDelay { get; set; }

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
            _storage = new TableTraitStorage(_data.storage);

            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableTrait));
        }
        protected TableTrait(TableTrait src, TableTraitCloneArgs args) : base(src)
        {
            _data = args.srcTraitDataClone;
            _owner = args.srcTraitOwnerClone;
            _storage = new TableTraitStorage(src._storage);
            
            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableTrait));
        }

        public override void Dispose()
        {
            base.Dispose();
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
