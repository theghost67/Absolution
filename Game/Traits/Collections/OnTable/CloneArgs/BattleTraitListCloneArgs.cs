using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования списка трейтов во время сражения.
    /// </summary>
    public class BattleTraitListCloneArgs : TableTraitListCloneArgs
    {
        public readonly new BattleTraitListSet srcListSetClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;

        public BattleTraitListCloneArgs(BattleTraitListSet srcListSetClone, BattleTerritoryCloneArgs terrCArgs)
            : base(srcListSetClone, terrCArgs)
        {
            this.srcListSetClone = srcListSetClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
