using Cysharp.Threading.Tasks;
using Game.Territories;

namespace Game.Cards
{
    public class cSatelliteSurveillance : FloatCard
    {
        const string ID = "satellite_surveillance";

        public cSatelliteSurveillance() : base(ID)
        {
            name = "Спутниковая слежка";
            desc = "Совместно разработанная с Агентством технология массовой слежки позволяет в считанные секунды найти любое существо на планете. " +
                   "Параллельно ведутся работы над техникой для орбитального удара, но это уже совсем другая история.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cSatelliteSurveillance(cSatelliteSurveillance other) : base(other) { }
        public override object Clone() => new cSatelliteSurveillance(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, "Раскрывает золото, эфир и руку противника до конца боя.");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            await base.OnUse(e);

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleSide opposite = card.Side.Opposite;

            opposite.Drawer.SleeveIsVisible = true;
            opposite.Drawer.WealthIsVisible = true;
        }
    }
}
