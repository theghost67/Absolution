using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования активного навыка на столе.
    /// </summary>
    public class TableActiveTraitCloneArgs : TableTraitCloneArgs
    {
        public readonly new ActiveTrait srcTraitDataClone;
        public TableActiveTraitCloneArgs(ActiveTrait srcTraitDataClone, TableFieldCard srcTraitOwnerClone, TableTerritoryCloneArgs terrCArgs)
            : base(srcTraitDataClone, srcTraitOwnerClone, terrCArgs)
        {
            this.srcTraitDataClone = srcTraitDataClone;
        }
    }
}
