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
        }
        protected cSatelliteSurveillance(cSatelliteSurveillance other) : base(other) { }
        public override object Clone() => new cSatelliteSurveillance(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return "Раскрывает руку противника до конца боя. Если рука противника уже раскрыта, раскрывает его золото и эфир.";
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

            if (opposite.Drawer == null) return;
            if (!opposite.Drawer.SleeveIsVisible)
                 opposite.Drawer.SleeveIsVisible = true;
            else opposite.Drawer.WealthIsVisible = true;
        }
    }
}
