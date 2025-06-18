using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Linq;

namespace Game.Cards
{
    public class cBlackFriday : FloatCard
    {
        const string ID = "black_friday";

        public cBlackFriday() : base(ID)
        {
            name = Translator.GetString("card_black_friday_1");
            desc = Translator.GetString("card_black_friday_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBlackFriday(cBlackFriday other) : base(other) { }
        public override object Clone() => new cBlackFriday(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return Translator.GetString("card_black_friday_3");
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleFloatCard> result)
        {
            return new(result.Entity, 1 + result.Entity.Side.CalculateCurrencyWeight("gold", result.Entity.Guid) * 2);
        }

        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            int cardsCount = card.Side.Fields().WithCard().ToArray().Length;
            await card.Side.Gold.AdjustValue(cardsCount, card);
        }
    }
}
