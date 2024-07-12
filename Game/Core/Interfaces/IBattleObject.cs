using Game.Territories;

namespace Game
{
    /// <summary>
    /// Реализует объект как объект сражения на территории, принадлежащий одной из сторон, который можно найти на территории.
    /// </summary>
    public interface IBattleObject : ITableObject, ITableEntrySource
    {
        public BattleSide Side { get; }
        public BattleTerritory Territory { get; }
    }
}
