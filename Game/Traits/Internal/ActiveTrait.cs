using Cysharp.Threading.Tasks;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс для всех активных трейтов. Эти трейты имеют цели и (возможно) условия активации.
    /// </summary>
    public abstract class ActiveTrait : Trait, IBattleActiveTraitThresholdUsable
    {
        public IBattleActiveTraitThresholdUsable Threshold => this;
        public ActiveTrait(string id) : base(id, isPassive: false) { }
        protected ActiveTrait(ActiveTrait other) : base(other) { }

        public sealed override TableTrait CreateOnTable(Transform parent)
        {
            return new TableActiveTrait(this, null, parent);
        }

        // see how weight threshold is used in BattleAI.CalculateWeightDeltas()
        public virtual BattleWeight GetWeightDeltaUseThreshold(BattleActiveTrait trait) => new(0, 0.12f);

        public virtual bool IsUsable(TableActiveTraitUseArgs e)
        {
            if (!e.isInBattle)
                return e.trait.Storage.turnsDelay <= 0;
            IBattleTrait trait = (IBattleTrait)e.trait;
            return trait.Territory.PhaseSide == trait.Side;
        }
        public virtual UniTask OnUse(TableActiveTraitUseArgs e)
        {
            return e.trait.AnimActivation();
        }
    }
}