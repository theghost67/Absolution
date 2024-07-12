using Game.Traits;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий результат взвешивания использования активного навыка во время сражения на цели.
    /// </summary>
    public class BattleActiveTraitWeightResult : BattleWeightResult<BattleActiveTrait>
    {
        public readonly BattleField target;
        public BattleActiveTraitWeightResult(BattleActiveTrait trait, BattleField target, float weightDeltaAbs, float weightDeltaRel) : base(trait, weightDeltaAbs, weightDeltaRel)
        {
            this.target = target;
        }
    }
}
