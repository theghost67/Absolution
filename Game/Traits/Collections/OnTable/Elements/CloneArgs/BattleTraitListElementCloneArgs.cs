using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования элемента списка трейтов во время сражения.
    /// </summary>
    public class BattleTraitListElementCloneArgs : TableTraitListElementCloneArgs
    {
        public readonly new IBattleTraitList srcListClone;
        public readonly new BattleTerritoryCloneArgs terrCArgs;

        public BattleTraitListElementCloneArgs(IBattleTraitList srcListClone, BattleTerritoryCloneArgs terrCArgs) 
            : base(srcListClone, terrCArgs)
        {
            this.srcListClone = srcListClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
