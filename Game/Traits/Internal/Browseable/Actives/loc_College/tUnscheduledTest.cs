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
        const string IGNORED_TRAIT_ID = "scholar";
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);
        static readonly TerritoryRange _range = TerritoryRange.oppositeAll;

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

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(IGNORED_TRAIT_ID).name;
            return $"<color>При использовании</color>\nТратит все заряды и испытывает каждую карту на территории напротив - " +
                   $"если её инициатива ≤ {_moxieF.Format(args.stacks)}, она получит {_strengthF.Format(args.stacks)} урона. Карты с навыком <u>{traitName}</u> не получают урона.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(IGNORED_TRAIT_ID) };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(6, stacks);
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
            IEnumerable<BattleField> fields = trait.Owner.Territory.Fields(trait.Owner.Field.pos, _range).WithCard();
            int moxie = _moxieF.ValueInt(e.traitStacks);
            int strength = _strengthF.ValueInt(e.traitStacks);

            await trait.SetStacks(0, trait.Side);
            foreach (BattleField field in fields)
            {
                BattleFieldCard card = field.Card;
                if (card.Moxie > moxie) continue;
                if (card.Traits.Passive("scholar") != null)
                {
                    card.Drawer.CreateTextAsSpeech("Ученик", ColorPalette.CP.ColorCur);
                    continue;
                }
                card.Drawer.CreateTextAsSpeech($"Кол\n<size=50%>-{strength}", Color.red);
                await card.Health.AdjustValue(-strength, trait);
            }
        }
    }
}
