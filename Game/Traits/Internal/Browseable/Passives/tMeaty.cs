using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tMeaty : PassiveTrait
    {
        const string ID = "meaty";
        const int PRIORITY = 8;
        const float HEALTH_REL_INCREASE = 0.25f;

        public tMeaty() : base(ID)
        {
            name = "Мясистый";
            desc = "Попробуй пробить вот такую защиту!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tMeaty(tMeaty other) : base(other) { }
        public override object Clone() => new tMeaty(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = HEALTH_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты с трейтом <i>Мясистый</i> рядом с владельцем (П{PRIORITY})",
                    $"увеличивает здоровье владельца на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 6 * Mathf.Pow(stacks - 1, 2);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            BattlePassiveTrait trait = (BattlePassiveTrait)e.trait;
            string entryId = $"{trait.Guid}/{e.target.Guid}";

            if (e.target.Traits.Passive(ID) == null) return;
            if (e.canSeeTarget)
            {
                await trait.AnimActivation();
                await trait.Owner.health.AdjustValueScale(HEALTH_REL_INCREASE * trait.GetStacks(), trait, entryId);
            }
        }
    }
}
