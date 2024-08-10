namespace Game.Cards
{
    /// <summary>
    /// Реализует объект стола как карту на столе.
    /// </summary>
    public interface ITableCard : ITableObject, ITableEntrySource, ICloneableWithArgs
    {
        public Card Data { get; }
        public TableStat Price { get; }
        public new TableCardDrawer Drawer { get; }
        Drawer ITableObject.Drawer => Drawer;
    }
}
