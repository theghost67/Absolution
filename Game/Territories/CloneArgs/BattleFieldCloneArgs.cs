using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования поля во время сражения.
    /// </summary>
    public class BattleFieldCloneArgs : TableFieldCloneArgs
    {
        public readonly new BattleTerritoryCloneArgs terrCArgs;
        public readonly BattleSide srcFieldSideClone;

        public BattleFieldCloneArgs(FieldCard srcFieldCardDataClone, BattleSide srcFieldSideClone, BattleTerritoryCloneArgs terrCArgs) 
            : base(srcFieldCardDataClone, terrCArgs)
        {
            this.srcFieldSideClone = srcFieldSideClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
