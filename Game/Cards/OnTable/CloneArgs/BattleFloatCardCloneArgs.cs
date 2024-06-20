using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования карты без характеристик во время сражения.
    /// </summary>
    public class BattleFloatCardCloneArgs : TableFloatCardCloneArgs
    {
        public readonly BattleSide srcCardSideClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;

        public BattleFloatCardCloneArgs(FloatCard srcCardDataClone, BattleSide srcCardSideClone, BattleTerritoryCloneArgs terrCArgs) 
            : base(srcCardDataClone, terrCArgs)
        {
            this.srcCardSideClone = srcCardSideClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
