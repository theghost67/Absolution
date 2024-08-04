using Cysharp.Threading.Tasks;
using Game.Territories;
using GreenOne;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс для данных карт без характеристик. Эти карты могут быть (а могут и не быть) единожды активированы на игровой территории (см. <see cref="TableTerritory"/>).
    /// </summary>
    public abstract class FloatCard : Card, IBattleThresholdUsable<BattleFloatCard>
    {
        public IBattleThresholdUsable<BattleFloatCard> Threshold => this;
        public FloatCard(string id) : base(id, isField: false) { }
        protected FloatCard(FloatCard other) : base(other) { }

        public override SerializationDict Serialize()
        {
            return base.Serialize();
        }
        public override TableCard CreateOnTable(Transform parent)
        {
            return new TableFloatCard(this, parent);
        }
        public override float Points() => default;

        public virtual BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleFloatCard> card) => new(0, 0.16f);
        public virtual bool IsUsable(TableFloatCardUseArgs e) => false;
        public virtual UniTask OnUse(TableFloatCardUseArgs e) => UniTask.CompletedTask;
    }
}