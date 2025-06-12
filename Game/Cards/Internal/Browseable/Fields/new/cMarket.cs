namespace Game.Cards
{
    public class cMarket : FieldCard
    {
        public cMarket() : base("market", "shopping")
        {
            name = "Чёрный рынок";
            desc = "Кто готов к шопингу?";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 0);

            frequency = 0;
        }
        protected cMarket(cMarket other) : base(other) { }
        public override object Clone() => new cMarket(this);
    }
}
