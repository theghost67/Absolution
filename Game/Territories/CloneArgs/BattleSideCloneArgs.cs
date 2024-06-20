namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования стороны сражения.
    /// </summary>
    public class BattleSideCloneArgs : CloneArgs
    {
        public readonly BattleTerritory srcSideTerritoryClone;
        public readonly BattleTerritoryCloneArgs terrCArgs;

        public BattleSideCloneArgs(BattleTerritory srcSideTerritoryClone, BattleTerritoryCloneArgs terrCArgs)
        {
            this.srcSideTerritoryClone = srcSideTerritoryClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
