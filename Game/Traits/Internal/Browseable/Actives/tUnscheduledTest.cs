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
            name = Translator.GetString("trait_unscheduled_test_1");
            desc = Translator.GetString("trait_unscheduled_test_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tUnscheduledTest(tUnscheduledTest other) : base(other) { }
        public override object Clone() => new tUnscheduledTest(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(IGNORED_TRAIT_ID).name;
            return Translator.GetString("trait_unscheduled_test_3", _moxieF.Format(args.stacks), _strengthF.Format(args.stacks), traitName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(IGNORED_TRAIT_ID) { linkFormat = true } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.12f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            IEnumerable<BattleField> fields = trait.Owner.Territory.Fields(trait.Owner.Field.pos, _range).WithCard();
            int moxie = _moxieF.ValueInt(e.traitStacks);
            int strength = _strengthF.ValueInt(e.traitStacks);

       
            foreach (BattleField field in fields)
            {
                BattleFieldCard card = field.Card;
                if (card.Moxie > moxie) continue;
                if (card.Traits.Passive("scholar") != null)
                {
                    card.Drawer.CreateTextAsSpeech(Translator.GetString("trait_unscheduled_test_4"), ColorPalette.CP.ColorCur);
                    continue;
                }
                card.Drawer.CreateTextAsSpeech(Translator.GetString("trait_unscheduled_test_5", strength), Color.red);
                await card.Health.AdjustValue(-strength, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
