using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWhiteBombing : ActiveTrait
    {
        const string ID = "white_bombing";
        const string CARD_ID = "pigeon_litter";
        const string TRAIT_ID = "unpleasant_scent";
        static readonly TraitStatFormula _stacksMoxieF = new(false, 0, 1);

        public tWhiteBombing() : base(ID)
        {
            name = "Белоснежная бомбардировка";
            desc = "Чёрт, это опять те голуби, бежим отсюда, пока не поздно!";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.bothSingle);
        }
        protected tWhiteBombing(tWhiteBombing other) : base(other) { }
        public override object Clone() => new tWhiteBombing(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            string format = _stacksMoxieF.Format(args.stacks);
            return $"<color>При активации на союзном поле</color>\nТратит все заряды, создаёт на всех пустых полях своей территории карты <nobr><color><u>{cardName}</u></color></nobr>.\n\n" +
                   $"<color>При активации на вражеском поле</color>\nТратит все заряды, инициатива всех карт на территории напротив будет уменьшена на {format}.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            TraitStacksPair[] traits = new TraitStacksPair[] { new(TRAIT_ID, args.stacks) };
            return new DescLinkCollection()
            { 
                new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats, linkTraits = traits },
                new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = traits[0].stacks },
            };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
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
            BattleField target = (BattleField)e.target;
            IBattleTrait trait = (IBattleTrait)e.trait;
            bool usedOnOwnerSide = trait.Side.isMe == target.Side.isMe;

            if (usedOnOwnerSide)
            {
                IEnumerable<BattleField> fields = trait.Side.Fields().WithoutCard();
                foreach (BattleField field in fields)
                {
                    FieldCard card = CardBrowser.NewField(CARD_ID);
                    card.traits.Passives.AdjustStacks(TRAIT_ID, _stacksMoxieF.ValueInt(e.traitStacks));
                    await trait.Side.Territory.PlaceFieldCard(card, field, trait.Side);
                }
            }
            else
            {
                IEnumerable<BattleField> fields = trait.Side.Opposite.Fields().WithCard();
                foreach (BattleField field in fields)
                    await field.Card.Moxie.AdjustValue(-_stacksMoxieF.ValueInt(e.traitStacks), trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
