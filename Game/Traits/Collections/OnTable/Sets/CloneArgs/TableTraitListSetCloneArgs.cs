using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования набора списков навыков на столе.
    /// </summary>
    public class TableTraitListSetCloneArgs : CloneArgs
    {
        public readonly TableFieldCard srcSetOwnerClone;
        public readonly TableTerritoryCloneArgs terrCArgs;
        public TableTraitListSetCloneArgs(TableFieldCard srcSetOwnerClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.srcSetOwnerClone = srcSetOwnerClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
