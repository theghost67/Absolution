namespace Game.Cards
{
    public class cSpider : FieldCard
    {
        public cSpider() : base("spider")
        {
            name = "Паук";
            desc = "Пушистый и злой комочек нашего паукообразного.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0;
        }
        protected cSpider(cSpider other) : base(other) { }
        public override object Clone() => new cSpider(this);
    }
}
