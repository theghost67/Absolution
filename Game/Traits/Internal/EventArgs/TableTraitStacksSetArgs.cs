namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для функций, вызываемых при изменении стаков навыков стола.
    /// </summary>
    public class TableTraitStacksSetArgs
    {
        public ITableTrait Trait => element.Trait;
        public int Stacks => element.Stacks;

        public readonly ITableTraitListElement element; // only traits attached to owner can invoke StacksSet functions
        public readonly int delta;
        public readonly ITableEntrySource source;
        public readonly bool isInBattle;

        public TableTraitStacksSetArgs(ITableTraitListElement element, int delta, ITableEntrySource source)
        {
            this.element = element;
            this.delta = delta;
            this.source = source;
            isInBattle = element is IBattleTraitListElement;
        }
    }
}
