using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLookOfDespair : PassiveTrait
    {
        const string ID = "look_of_despair";
        const int PRIORITY = 7;
        const int MOXIE_DECREASE = 5;

        public tLookOfDespair() : base(ID)
        {
            name = "Взгляд отчаяния";
            desc = "Я видел некоторое дерьмо...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tLookOfDespair(tLookOfDespair other) : base(other) { }
        public override object Clone() => new tLookOfDespair(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = MOXIE_DECREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты напротив владельца (П{PRIORITY})",
                    $"уменьшает её инициативу на <u>{effect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 16 * Mathf.Pow(stacks - 1, 2);
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            BattlePassiveTrait trait = (BattlePassiveTrait)e.trait;
            string entryId = $"{trait.Guid}/{e.target.Guid}";

            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await e.target.moxie.AdjustValue(-MOXIE_DECREASE * trait.GetStacks(), trait, entryId);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await e.target.moxie.RevertValue(entryId);
            }
        }
    }
}
