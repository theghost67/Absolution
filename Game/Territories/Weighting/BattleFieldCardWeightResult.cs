using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий результат взвешивания установки карты поля во время сражения на определённое поле.
    /// </summary>
    public class BattleFieldCardWeightResult : BattleWeightResult<BattleFieldCard>
    {
        public readonly BattleField field;
        public BattleFieldCardWeightResult(BattleFieldCard card, BattleField field, float weightDeltaAbs, float weightDeltaRel) : base(card, weightDeltaAbs, weightDeltaRel)
        {
            this.field = field;
            if (card.Side.CanAfford(card)) return;

            float diffModifier = UnityEngine.Mathf.Pow(2, card.Side.GetCurrencyDifference(card));
            base.weightDeltaAbs /= diffModifier;
            base.weightDeltaRel /= diffModifier;
        }
    }
}
