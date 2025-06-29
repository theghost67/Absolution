using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tReporting : ActiveTrait
    {
        const string ID = "reporting";
        const string CARD_ID = "clues";

        public tReporting() : base(ID)
        {
            name = Translator.GetString("trait_reporting_1");
            desc = Translator.GetString("trait_reporting_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tReporting(tReporting other) : base(other) { }
        public override object Clone() => new tReporting(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_reporting_3", cardName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(6, stacks, 1, 1.125f);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.1f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Opposite.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField newCardField = (BattleField)e.target.Opposite;
            FieldCard newCard = CardBrowser.NewField(CARD_ID);

            await trait.Territory.PlaceFieldCard(newCard, newCardField, trait);
            await trait.AdjustStacks(-1, trait.Side);
        }
    }
}
