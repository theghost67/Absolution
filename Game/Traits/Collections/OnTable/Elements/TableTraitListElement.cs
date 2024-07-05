namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий один из элементов списка трейтов на столе (см. <see cref="ITableTraitList"/>).
    /// </summary>
    public abstract class TableTraitListElement : TableObject, ITableTraitListElement
    {
        public int Stacks { get => _stacks; }
        public new TableTraitListElementDrawer Drawer => ((TableObject)this).Drawer as TableTraitListElementDrawer;

        public override string TableName => _trait.TableName;
        public override string TableNameDebug => _trait.TableNameDebug;

        public ITableTraitList List => _list;
        public ITableTrait Trait => _trait;
        public ITableEntryDict StacksEntries => _stacksEntries;

        readonly ITableTraitList _list;
        readonly ITableTrait _trait;
        readonly TableEntryDict _stacksEntries;
        int _stacks;

        public TableTraitListElement(ITableTraitList list, ITableTrait trait) : base(list.Set.Drawer?.transform)
        {
            _list = list;
            _trait = trait;
            _stacksEntries = new TableEntryDict();
            TryOnInstantiatedAction(GetType(), typeof(TableTraitListElement));
        }
        protected TableTraitListElement(TableTraitListElement src, TableTraitListElementCloneArgs args)
        {
            _list = args.srcListClone;
            _trait = TraitCloner(src._trait, args);

            TableEntryDictCloneArgs entriesCArgs = new(args.terrCArgs);
            _stacksEntries = (TableEntryDict)src._stacksEntries.Clone(entriesCArgs);
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

        public void Dispose()
        {
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
