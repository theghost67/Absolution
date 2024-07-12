using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Реализует навык на столе как навык во время сражения с возможностью применения на целях.
    /// </summary>
    public interface IBattleTrait : ITableTrait, IBattleWeighty
    {
        public new BattleFieldCard Owner { get; }
        TableFieldCard ITableTrait.Owner => Owner;
    }
}
