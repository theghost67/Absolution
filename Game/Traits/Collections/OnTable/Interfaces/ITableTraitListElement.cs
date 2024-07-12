using System;

namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как один из элементов списка навыков на столе (см. <see cref="ITableTraitList"/>) без возможности изменения стаков.
    /// </summary>
    public interface ITableTraitListElement : ITableObject, IEquatable<ITableTraitListElement>, IEquatable<ITableTrait>, ICloneableWithArgs
    {
        public ITableTraitList List { get; }
        public ITableTrait Trait { get; }
        public ITableEntryDict StacksEntries { get; }
        public int Stacks { get; }

        public new TableTraitListElementDrawer Drawer { get; }
        Drawer ITableObject.Drawer => Drawer;
    }
}
