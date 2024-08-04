using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using GreenOne;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tUnscheduledTest : ActiveTrait
    {
        const string ID = "unscheduled_test";
        const int MOXIE_THRESHOLD = 2;
        const float STRENGTH_PER_STACK = 4f;
        const string IGNORED_TRAIT_ID = "scholar";
        static readonly TerritoryRange targets = TerritoryRange.oppositeAll;

        public tUnscheduledTest() : base(ID)
        {
            name = "Внеплановая контрольная";
            desc = "Начинаем контрольную работу!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tUnscheduledTest(tUnscheduledTest other) : base(other) { }
        public override object Clone() => new tUnscheduledTest(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(IGNORED_TRAIT_ID).name;
            float effect = STRENGTH_PER_STACK * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории",
                    $"тратит все заряды и испытывает каждую карту на территории напротив - если её инициатива < {MOXIE_THRESHOLD}. она получит <u>{effect}</u> ед. урона. Карты с навыком <i>{traitName}</i> не получают урона."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + (12 * (stacks - 1));
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.12f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            IEnumerable<BattleField> fields = trait.Owner.Territory.Fields(trait.Owner.Field.pos, targets).WithCard();
            int strength = (STRENGTH_PER_STACK * trait.GetStacks()).Ceiling();

            foreach (BattleField field in fields)
            {
                BattleFieldCard card = field.Card;
                if (card.moxie >= MOXIE_THRESHOLD) continue;
                if (card.Traits.Passive("scholar") != null)
                {
                    card.Drawer.CreateTextAsSpeech("Ученик", ColorPalette.CP.ColorCur);
                    continue;
                }
                card.Drawer.CreateTextAsSpeech($"Кол\n<size=50%>-{strength}", Color.red);
                await card.health.AdjustValue(-strength, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
