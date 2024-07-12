using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования активного навыка во время сражения.
    /// </summary>
    public class BattleActiveTraitCloneArgs : TableActiveTraitCloneArgs
    {
        public readonly new ActiveTrait srcTraitDataClone;
        public readonly new BattleFieldCard srcTraitOwnerClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;

        public BattleActiveTraitCloneArgs(ActiveTrait srcTraitDataClone, BattleFieldCard srcTraitOwnerClone, BattleTerritoryCloneArgs terrCArgs) 
            : base(srcTraitDataClone, srcTraitOwnerClone, terrCArgs)
        {
            this.srcTraitDataClone = srcTraitDataClone;
            this.srcTraitOwnerClone = srcTraitOwnerClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
