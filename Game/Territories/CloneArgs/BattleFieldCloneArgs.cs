namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования поля во время сражения.
    /// </summary>
    public class BattleFieldCloneArgs : TableFieldCloneArgs
    {
        public readonly new BattleTerritory srcTerrClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;
        public readonly BattleSide srcFieldSideClone;

        public BattleFieldCloneArgs(BattleSide srcFieldSideClone, BattleTerritory srcTerrClone, BattleTerritoryCloneArgs terrCArgs)
            : base(srcTerrClone, terrCArgs)
        {
            this.srcTerrClone = srcTerrClone;
            this.srcFieldSideClone = srcFieldSideClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
