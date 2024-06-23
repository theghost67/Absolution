using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using GreenOne;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tUnscheduledTest : ActiveTrait
    {
        const string ID = "unscheduled_test";
        const int MOXIE_THRESHOLD = 2;
        const float OWNER_STRENGTH_SCALE = 0.5f;

        public tUnscheduledTest() : base(ID)
        {
            name = "Внеплановая контрольная";
            desc = "Начинаем контрольную работу!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerSingle, TerritoryRange.oppositeAll);
        }
        protected tUnscheduledTest(tUnscheduledTest other) : base(other) { }
        public override object Clone() => new tUnscheduledTest(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = OWNER_STRENGTH_SCALE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При активации (Т)",
                    $"тратит все заряды испытывает каждую карту на территории напротив - если её инициатива будет меньше {MOXIE_THRESHOLD}. она получит урон, равный <u>{effect}%</u> от силы этой карты. Карты с трейтом <i>Ученик</i> не получают урона."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 12 * Mathf.Pow(stacks, 2);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            IEnumerable<BattleField> fields = trait.Owner.Territory.Fields(trait.Owner.Field.pos, range.splash).WithCard();
            int strength = (OWNER_STRENGTH_SCALE * trait.GetStacks() * trait.Owner.strength).Ceiling();

            foreach (BattleField field in fields)
            {
                BattleFieldCard card = field.Card;
                if (card.moxie >= MOXIE_THRESHOLD) continue;
                if (card.Traits.Passive("scholar") != null)
                {
                    card.Drawer.CreateTextAsSpeech("Ученик", ColorPalette.GetColor(5));
                    continue;
                }

                card.Drawer.CreateTextAsSpeech("Кол", Color.red);
                await card.health.AdjustValueAbs(-strength, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
