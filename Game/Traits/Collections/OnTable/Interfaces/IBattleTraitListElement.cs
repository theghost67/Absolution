namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как один из элементов списка трейтов во время сражения (см. <see cref="IBattleTraitList"/>).
    /// </summary>
    public interface IBattleTraitListElement : ITableTraitListElement
    {
        public new IBattleTraitList List { get; }
        ITableTraitList ITableTraitListElement.List => List;

        public new IBattleTrait Trait { get; }
        ITableTrait ITableTraitListElement.Trait => Trait;
    }
}
