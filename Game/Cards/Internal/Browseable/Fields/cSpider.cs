namespace Game.Cards
{
    public class cSpider : FieldCard
    {
        public cSpider() : base("spider")
        {
            name = Translator.GetString("card_spider_1");
            desc = Translator.GetString("card_spider_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cSpider(cSpider other) : base(other) { }
        public override object Clone() => new cSpider(this);
    }
}
