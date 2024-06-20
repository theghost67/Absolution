using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования пассивного трейта во время сражения.
    /// </summary>
    public class BattlePassiveTraitCloneArgs : TablePassiveTraitCloneArgs
    {
        public readonly new PassiveTrait srcTraitDataClone;
        public readonly new BattleFieldCard srcTraitOwnerClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;

        public BattlePassiveTraitCloneArgs(PassiveTrait srcTraitDataClone, BattleFieldCard srcTraitOwnerClone, BattleTerritoryCloneArgs terrCArgs) 
            : base(srcTraitDataClone, srcTraitOwnerClone, terrCArgs)
        {
            this.srcTraitDataClone = srcTraitDataClone;
            this.srcTraitOwnerClone = srcTraitOwnerClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
