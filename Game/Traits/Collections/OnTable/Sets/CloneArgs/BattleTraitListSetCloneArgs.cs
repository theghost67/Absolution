using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования набора списков трейтов во время сражения.
    /// </summary>
    public class BattleTraitListSetCloneArgs : TableTraitListSetCloneArgs
    {
        public readonly new BattleFieldCard srcSetOwnerClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;

        public BattleTraitListSetCloneArgs(BattleFieldCard srcSetOwnerClone, BattleTerritoryCloneArgs terrCArgs)
            : base(srcSetOwnerClone, terrCArgs)
        {
            this.srcSetOwnerClone = srcSetOwnerClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
