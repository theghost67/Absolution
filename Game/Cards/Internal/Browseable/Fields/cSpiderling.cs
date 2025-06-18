namespace Game.Cards
{
    public class cSpiderling : FieldCard
    {
        public cSpiderling() : base("spiderling", "weaver")
        {
            name = Translator.GetString("card_spiderling_1");
            desc = Translator.GetString("card_spiderling_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cSpiderling(cSpiderling other) : base(other) { }
        public override object Clone() => new cSpiderling(this);
    }
}
