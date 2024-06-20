using System;

namespace Game.Cards
{
    /// <summary>
    /// Реализует объект стола как карту на столе.
    /// </summary>
    public interface ITableCard : ITableDrawable, ITableEntrySource, ICloneableWithArgs, IUnique, IDisposable
    {
        public Card Data { get; }
        public new TableCardDrawer Drawer { get; }
        Drawer ITableDrawable.Drawer => Drawer;
    }
}
