using Game.Traits;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий результат взвешивания использования активного навыка во время сражения на цели.
    /// </summary>
    public class BattleActiveTraitWeightResult : BattleWeightResult<BattleActiveTrait>
    {
        public BattleActiveTraitWeightResult(BattleActiveTrait trait, BattleField target, float weightDeltaAbs, float weightDeltaRel) 
            : base(trait, target, weightDeltaAbs, weightDeltaRel) { }
    }
}
