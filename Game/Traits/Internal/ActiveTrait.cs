using Cysharp.Threading.Tasks;
using Game.Sleeves;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс для всех активных навыков. Эти навыкы имеют цели и (возможно) условия активации.
    /// </summary>
    public abstract class ActiveTrait : Trait, IBattleThresholdUsable<BattleActiveTrait>
    {
        public IBattleThresholdUsable<BattleActiveTrait> Threshold => this;

        public ActiveTrait(string id) : base(id, isPassive: false) { }
        protected ActiveTrait(ActiveTrait other) : base(other) { }

        public sealed override TableTrait CreateOnTable(Transform parent) => new TableActiveTrait(this, null, parent);
        public virtual BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result) => BattleWeight.Zero(result.Entity);

        public virtual bool IsUsableInSleeve() => false;
        public virtual bool IsUsable(TableActiveTraitUseArgs e)
        {
            bool cooldown = e.trait.IsOnCooldown();
            if (cooldown) return false;

            ITableSleeveCard sleeveCard = e.trait.Owner as ITableSleeveCard;
            bool usedInSleeve = sleeveCard?.Sleeve.Contains(sleeveCard) ?? false;
            if (usedInSleeve && !IsUsableInSleeve()) return false;

            if (e.isInBattle)
            {
                IBattleTrait trait = (IBattleTrait)e.trait;
                return trait.Territory.PhaseSide == trait.Side;
            }

            bool targetIsReachable = e.trait.Owner.Field != null && !range.potential.OverlapsTarget(e.trait.Owner.Field.pos, e.target.pos);
            return usedInSleeve || targetIsReachable;
        }

        public async UniTask Use(TableActiveTraitUseArgs e)
        {
            await e.AnimActivation();
            await OnUse(e);
        }
        protected abstract UniTask OnUse(TableActiveTraitUseArgs e);
    }
}