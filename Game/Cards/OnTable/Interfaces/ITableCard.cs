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
        public int TurnAge { get; set; } // has not 0 value only if card is on BattleTerritory and turn was ended once
        Drawer ITableObject.Drawer => Drawer;
    }
}
