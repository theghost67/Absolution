namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования поля стола.
    /// </summary>
    public class TableFieldCloneArgs : CloneArgs
    {
        public readonly TableTerritory srcTerrClone;
        public readonly TableTerritoryCloneArgs terrCArgs;

        public TableFieldCloneArgs(TableTerritory srcTerrClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.srcTerrClone = srcTerrClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
