using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования пассивного навыка на столе.
    /// </summary>
    public class TablePassiveTraitCloneArgs : TableTraitCloneArgs
    {
        public readonly new PassiveTrait srcTraitDataClone;
        public TablePassiveTraitCloneArgs(PassiveTrait srcTraitDataClone, TableFieldCard srcTraitOwnerClone, TableTerritoryCloneArgs terrCArgs)
            : base(srcTraitDataClone, srcTraitOwnerClone, terrCArgs)
        {
            this.srcTraitDataClone = srcTraitDataClone;
        }
    }
}
