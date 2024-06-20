using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Реализует трейт на столе как трейт во время сражения с возможностью применения на целях.
    /// </summary>
    public interface IBattleTrait : ITableTrait, IBattleWeighty
    {
        public new BattleFieldCard Owner { get; }
        TableFieldCard ITableTrait.Owner => Owner;
    }
}
