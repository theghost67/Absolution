using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования списка трейтов на столе.
    /// </summary>
    public class TableTraitListCloneArgs : CloneArgs
    {
        public readonly TableTraitListSet srcListSetClone;
        public readonly TableTerritoryCloneArgs terrCArgs;

        public TableTraitListCloneArgs(TableTraitListSet srcListSetClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.srcListSetClone = srcListSetClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
