namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий один из элементов списка навыков на столе (см. <see cref="ITableTraitList"/>).
    /// </summary>
    public abstract class TableTraitListElement : TableObject, ITableTraitListElement
    {
        public new TableTraitListElementDrawer Drawer => ((TableObject)this).Drawer as TableTraitListElementDrawer;
        public override string TableName => _trait.TableName;
        public override string TableNameDebug => _trait.TableNameDebug;

        public ITableTraitList List => _list;
        public ITableTrait Trait => _trait;
        public ITableEntryDict StacksEntries => _stacksEntries;
        public int Stacks => _stacks;

        readonly ITableTraitList _list;
        readonly ITableTrait _trait;
        readonly TableEntryDict _stacksEntries;
        int _stacks;

        public TableTraitListElement(ITableTraitList list, ITableTrait trait, int stacks) : base(list.Set.Drawer?.transform)
        {
            _list = list;
            _trait = trait;
            _stacks = stacks;
            _stacksEntries = new TableEntryDict();
            TryOnInstantiatedAction(GetType(), typeof(TableTraitListElement));
        }
        protected TableTraitListElement(TableTraitListElement src, TableTraitListElementCloneArgs args)
        {
            _list = args.srcListClone;
            _trait = TraitCloner(src._trait, args);
            _stacks = src._stacks;
            _stacksEntries = (TableEntryDict)src._stacksEntries.Clone(new TableEntryDictCloneArgs(args.terrCArgs));
            TryOnInstantiatedAction(GetType(), typeof(TableTraitListElement));
        }

        public bool Equals(ITableTraitListElement other)
        {
            return Equals(other.Trait);
        }
        public bool Equals(ITableTrait trait)
        {
            return (_trait.Guid == trait.Guid) && (_trait.Owner.Guid == trait.Owner.Guid);
        }

        public override void Dispose()
        {
            base.Dispose();
            _stacksEntries.Clear();
            _trait.Dispose();
            Drawer?.Dispose();
        }
        public abstract object Clone(CloneArgs args);
        protected abstract ITableTrait TraitCloner(ITableTrait src, TableTraitListElementCloneArgs args);

        // NOTE 1: used only inside of the TableTraitList instance
        // NOTE 2: can be used for delta calculations (first call with positive value, second call with negative)
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
        }
    }
}
