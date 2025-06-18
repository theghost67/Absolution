using Cysharp.Threading.Tasks;
using Game.Territories;

namespace Game.Cards
{
    public class cSatelliteSurveillance : FloatCard
    {
        const string ID = "satellite_surveillance";

        public cSatelliteSurveillance() : base(ID)
        {
            name = Translator.GetString("card_satellite_surveillance_1");
            desc = Translator.GetString("card_satellite_surveillance_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cSatelliteSurveillance(cSatelliteSurveillance other) : base(other) { }
        public override object Clone() => new cSatelliteSurveillance(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return Translator.GetString("card_satellite_surveillance_3");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleSide opposite = card.Side.Opposite;

            if (opposite.Drawer == null) return;
            if (!opposite.Drawer.SleeveIsVisible)
                 opposite.Drawer.SleeveIsVisible = true;
            else opposite.Drawer.WealthIsVisible = true;
        }
    }
}
