using Game.Cards;

namespace Game.Sleeves
{
    /// <summary>
    /// Реализует карту рукава на столе как карту рукава во время сражения, добавляя возможность подбирать, возвращать и бросать карту на поле из рукава.
    /// </summary>
    public interface IBattleSleeveCard : ITableSleeveCard, IBattleCard 
    {
        public new BattleSleeve Sleeve { get; }
        TableSleeve ITableSleeveCard.Sleeve => Sleeve;
    }
}
