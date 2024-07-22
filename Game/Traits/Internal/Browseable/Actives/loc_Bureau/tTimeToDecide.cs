using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTimeToDecide : ActiveTrait
    {
        const string ID = "time_to_decide";
        const string TRAIT_ID = "time_to_decide_machine";
        const int MOXIE_ABS_INCREASE = 2;
        const float HEALTH_REL_INCREASE = 0.50f;
        const float STRENGTH_REL_INCREASE = 0.50f;

        public tTimeToDecide() : base(ID)
        {
            name = "Время решать, Коннор";
            desc = "- Не делай этого, Коннор.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.bothSingle);
        }
        protected tTimeToDecide(tTimeToDecide other) : base(other) { }
        public override object Clone() => new tTimeToDecide(this);

        public override string DescRich(ITableTrait trait)
        {
            int moxie = MOXIE_ABS_INCREASE * trait.GetStacks();
            float health = HEALTH_REL_INCREASE * 100 * trait.GetStacks();
            float strength = STRENGTH_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на союзном поле",
                    $"Становится девиантом, получая <u>{moxie}</u> ед. инициативы или вдвое больше, если на союзных полях есть карты с таким же навыком. Тратит все заряды."),
                new($"При использовании на вражеском поле",
                    $"Остаётся машиной, получая бонус в <u>{health}%</u> к здоровью и <u>{strength}%</u> к силе. Вероятны негативные последствия. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 160 * Mathf.Pow(stacks - 1, 2);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            BattleFieldCard owner = trait.Owner;

            bool isAllyField = e.target.IsMine();
            if (isAllyField)
            {
                int moxie = MOXIE_ABS_INCREASE * trait.GetStacks();
                await owner.moxie.AdjustValue(moxie, trait);
            }
            else
            {
                float health = HEALTH_REL_INCREASE * trait.GetStacks();
                float strength = STRENGTH_REL_INCREASE * trait.GetStacks();
                await owner.health.AdjustValueScale(health, trait);
                await owner.strength.AdjustValueScale(strength, trait);
                await owner.Traits.AdjustStacks(TRAIT_ID, 1, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
