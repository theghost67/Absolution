using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования карты поля во время сражения.
    /// </summary>
    public class BattleFieldCardCloneArgs : TableFieldCardCloneArgs
    {
        public readonly new BattleTerritoryCloneArgs terrCArgs;
        public readonly BattleSide srcCardSideClone;

        public BattleFieldCardCloneArgs(FieldCard srcCardDataClone, BattleField srcCardFieldClone, BattleSide srcCardSideClone, BattleTerritoryCloneArgs terrCArgs)
            : base(srcCardDataClone, srcCardFieldClone, terrCArgs)
        {
            this.srcCardSideClone = srcCardSideClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
