namespace Game.Cards
{
    public class cSpiderCocon : FieldCard
    {
        public cSpiderCocon() : base("spider_cocon")
        {
            name = Translator.GetString("card_spider_cocon_1");
            desc = Translator.GetString("card_spider_cocon_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);

            frequency = 0;
        }
        protected cSpiderCocon(cSpiderCocon other) : base(other) { }
        public override object Clone() => new cSpiderCocon(this);
    }
}
