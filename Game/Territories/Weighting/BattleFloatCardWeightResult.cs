using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий результат взвешивания использования карты без характеристик во время сражения.
    /// </summary>
    public class BattleFloatCardWeightResult : BattleWeightResult<BattleFloatCard>
    {
        public BattleFloatCardWeightResult(BattleFloatCard card, float weightDeltaAbs, float weightDeltaRel) 
            : base(card, null, weightDeltaAbs, weightDeltaRel) 
        {
            if (card == null) return;
            if (card.Side.CanAfford(card)) return;
            float diffModifier = UnityEngine.Mathf.Pow(2, card.Side.GetCurrencyDifference(card));
            base.weightDeltaAbs /= diffModifier;
            base.weightDeltaRel /= diffModifier;
        }
    }
}
