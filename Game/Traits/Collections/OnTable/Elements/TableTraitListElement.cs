using GreenOne;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий один из элементов списка трейтов на столе (см. <see cref="ITableTraitList"/>).
    /// </summary>
    public abstract class TableTraitListElement : ITableTraitListElement
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public IIdEventVoid<int> OnStacksChanged => _onStacksChanged;
        public int Stacks { get => _stacks; }
        public TableTraitListElementDrawer Drawer => _drawer;

        public ITableTraitList List => _list;
        public ITableTrait Trait => _trait;
        public ITableEntryDict StacksEntries => _stacksEntries;

        readonly IdEventVoid<int> _onStacksChanged;
        readonly ITableTraitList _list;
        readonly ITableTrait _trait;
        readonly TableEntryDict _stacksEntries;

        int _stacks;
        TableTraitListElementDrawer _drawer;

        public TableTraitListElement(ITableTraitList list, ITableTrait trait, bool withDrawer = true)
        {
            _onStacksChanged = new IdEventVoid<int>();
            _list = list;
            _trait = trait;
            _stacksEntries = new TableEntryDict();

            if (withDrawer)
                CreateDrawer(null);
        }
        protected TableTraitListElement(TableTraitListElement src, TableTraitListElementCloneArgs args)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            _onStacksChanged = (IdEventVoid<int>)src._onStacksChanged.Clone();
            _list = args.srcListClone;
            _trait = TraitCloner(src._trait, args);

            TableEntryDictCloneArgs entriesCArgs = new(args.terrCArgs);
            _stacksEntries = (TableEntryDict)src._stacksEntries.Clone(entriesCArgs);
        }

        public bool Equals(ITableTraitListElement other)
        {
            return Equals(other.Trait);
        }
        public bool Equals(ITableTrait trait)
        {
            return (_trait.Guid == trait.Guid) && (_trait.Owner.Guid == trait.Owner.Guid);
        }

        public void CreateDrawer(Transform parent)
        {
            if (_drawer != null) return;
            TableTraitListElementDrawer drawer = DrawerCreator(parent);
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

        public void Dispose()
        {
            _stacksEntries.Clear();
            _trait.Dispose();
            _drawer?.Dispose();
        }
        public abstract object Clone(CloneArgs args);

        protected abstract ITableTrait TraitCloner(ITableTrait src, TableTraitListElementCloneArgs args);
        protected virtual void DrawerSetter(TableTraitListElementDrawer value)
        {
            _drawer = value;
        }
        protected abstract TableTraitListElementDrawer DrawerCreator(Transform parent);

        // use only inside of the TableTraitList instance
        public void AddEntryInternal(string id, TableEntry entry)
        {
            _stacksEntries.Add(id, entry);
        }
        public bool RemoveEntryInternal(string id)
        {
            return _stacksEntries.Remove(id);
        }
        public void AdjustStacksInternal(int delta)
        {
            _stacks += delta;
            _onStacksChanged.Invoke(this, delta);
        }
    }
}
