using Game.Territories;

namespace Game
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования характеристики стола.
    /// </summary>
    public class TableStatCloneArgs : CloneArgs
    {
        public readonly object ownerClone;
        public readonly TableTerritoryCloneArgs terrCArgs;

        public TableStatCloneArgs(object ownerClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.ownerClone = ownerClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
