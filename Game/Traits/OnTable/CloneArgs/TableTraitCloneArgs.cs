using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования любого трейта на столе.
    /// </summary>
    public class TableTraitCloneArgs : CloneArgs
    {
        public readonly Trait srcTraitDataClone;
        public readonly TableFieldCard srcTraitOwnerClone;
        public readonly TableTerritoryCloneArgs terrCArgs;

        public TableTraitCloneArgs(Trait srcTraitDataClone, TableFieldCard srcTraitOwnerClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.srcTraitDataClone = srcTraitDataClone;
            this.srcTraitOwnerClone = srcTraitOwnerClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
