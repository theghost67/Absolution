using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System.Linq;

namespace Game.Cards
{
    public class cNoed : FloatCard
    {
        const string ID = "noed";
        const string TRAIT_ID = "exposed";

        public cNoed() : base(ID)
        {
            name = Translator.GetString("card_noed_1");
            desc = Translator.GetString("card_noed_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cNoed(cNoed other) : base(other) { }
        public override object Clone() => new cNoed(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("card_noed_3", traitName);
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            BattleFieldCard[] cards = card.Side.Opposite.Fields().WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard c in cards)
                await c.Traits.Passives.AdjustStacks(TRAIT_ID, 1, card);
        }
    }
}
