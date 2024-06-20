using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования элемента списка трейтов на столе.
    /// </summary>
    public class TableTraitListElementCloneArgs : CloneArgs
    {
        public readonly ITableTraitList srcListClone;
        public readonly TableTerritoryCloneArgs terrCArgs;

        public TableTraitListElementCloneArgs(ITableTraitList srcListClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.srcListClone = srcListClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
