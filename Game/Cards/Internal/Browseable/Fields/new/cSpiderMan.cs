namespace Game.Cards
{
    public class cSpiderMan : FieldCard
    {
        public cSpiderMan() : base("spider_man", "spider_suit", "amazement")
        {
            name = Translator.GetString("card_spider_man_1");
            desc = Translator.GetString("card_spider_man_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cSpiderMan(cSpiderMan other) : base(other) { }
        public override object Clone() => new cSpiderMan(this);
    }
}
