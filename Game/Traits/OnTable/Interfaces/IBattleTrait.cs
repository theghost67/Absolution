using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Реализует навык на столе как навык во время сражения с возможностью применения на целях.
    /// </summary>
    public interface IBattleTrait : ITableTrait, IBattleObject, IBattleRanging
    {
        public new BattleFieldCard Owner { get; }
        public new BattleTerritory Territory { get; }
        public new BattleField Field { get; }

        TableFieldCard ITableTrait.Owner => Owner;
        TableTerritory ITableTrait.Territory => Territory;
        TableField ITableTrait.Field => Field;
    }
}
