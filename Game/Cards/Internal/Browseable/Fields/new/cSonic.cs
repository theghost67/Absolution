namespace Game.Cards
{
    public class cSonic : FieldCard
    {
        public cSonic() : base("sonic", "gotta_go", "lightning_speed")
        {
            name = "Соник";
            desc = "Сверхскоростной синий ёж из дальних миров, прибывший на Землю ради того, чтобы поесть чипсов и посмотреть бейсбол. " +
                   "Благодаря своей скорости, он может менять своё окружение под себя. И избавлять себя от проблем, разумеется! Совет от Соника - беги от слоника.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 0);
        }
        protected cSonic(cSonic other) : base(other) { }
        public override object Clone() => new cSonic(this);
    }
}
