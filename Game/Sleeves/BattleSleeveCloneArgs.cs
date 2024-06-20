using Game.Territories;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования рукава во время сражения.
    /// </summary>
    public class BattleSleeveCloneArgs : TableSleeveCloneArgs
    {
        public readonly BattleSide srcSleeveSideClone;
        public readonly BattleTerritoryCloneArgs terrCArgs;

        public BattleSleeveCloneArgs(BattleSide srcSleeveSideClone, BattleTerritoryCloneArgs terrCArgs) : base(srcSleeveSideClone.Deck) 
        { 
            this.srcSleeveSideClone = srcSleeveSideClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
