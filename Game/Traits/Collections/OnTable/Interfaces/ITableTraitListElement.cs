using System;

namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как один из элементов списка трейтов на столе (см. <see cref="ITableTraitList"/>) без возможности изменения стаков.
    /// </summary>
    public interface ITableTraitListElement : ITableDrawable, IEquatable<ITableTraitListElement>, IEquatable<ITableTrait>, ICloneableWithArgs, IDisposable
    {
        public ITableTraitList List { get; }
        public ITableTrait Trait { get; }
        public ITableEntryDict StacksEntries { get; }
        public int Stacks { get; }

        public new TableTraitListElementDrawer Drawer { get; }
        Drawer ITableDrawable.Drawer => Drawer;
    }
}
