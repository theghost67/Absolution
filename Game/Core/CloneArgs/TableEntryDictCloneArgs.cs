using Game.Territories;

namespace Game
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования словаря записей.
    /// </summary>
    public class TableEntryDictCloneArgs : CloneArgs
    {
        public readonly TableTerritoryCloneArgs terrCArgs;
        public TableEntryDictCloneArgs(TableTerritoryCloneArgs terrCArgs)
        {
            this.terrCArgs = terrCArgs;
        }
    }
}
