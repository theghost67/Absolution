using Cysharp.Threading.Tasks;
using Game.Cards;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс для любого трейта, находящегося на столе.
    /// </summary>
    public abstract class TableTrait : Unique, ITableTrait
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public Trait Data => _data;
        public TableFieldCard Owner => _owner;
        public TableTraitStorage Storage => _storage;
        public TableTraitDrawer Drawer => _drawer;
        public virtual TableFinder Finder => null;
        public string TableName => $"{Data.name}[{Owner.Field.PosToStringRich()}]";

        readonly Trait _data;
        readonly TableFieldCard _owner;
        readonly TableTraitStorage _storage;
        TableTraitDrawer _drawer;

        public TableTrait(Trait data, TableFieldCard owner, Transform parent, bool withDrawer = true): base()
        {
            _data = data;
            _owner = owner;
            _storage = new TableTraitStorage(this, _data.storage);

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TableTrait(TableTrait src, TableTraitCloneArgs args) : base(src.Guid)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            _data = args.srcTraitDataClone;
            _owner = args.srcTraitOwnerClone;
            _storage = new TableTraitStorage(this, src._storage);
        }

        public virtual void Dispose()
        {
            _drawer?.Dispose();
            _storage.Clear();
        }
        public abstract object Clone(CloneArgs args);

        public void CreateDrawer(Transform parent)
        {
            if (_drawer != null) return;
            TableTraitDrawer drawer = DrawerCreator(parent);
            DrawerSetter(drawer);
            OnDrawerCreated?.Invoke(this, EventArgs.Empty);
        }
        public void DestroyDrawer(bool instantly)
        {
            if (_drawer == null) return;
            _drawer.TryDestroy(instantly);
            DrawerSetter(null);
            OnDrawerDestroyed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DrawerSetter(TableTraitDrawer value)
        {
            _drawer = value;
        }
        protected virtual TableTraitDrawer DrawerCreator(Transform parent)
        {
            return new TableTraitDrawer(this, parent);
        }

        public abstract int GetStacks();
        public abstract UniTask AdjustStacks(int delta, ITableEntrySource source);
        public UniTask SetStacks(int value, ITableEntrySource source)
        {
            return AdjustStacks(value - GetStacks(), source);
        }
    }
}
