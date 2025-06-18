namespace Game.Cards
{
    public class cCrap : FieldCard
    {
        public cCrap() : base("crap")
        {
            name = Translator.GetString("card_crap_1");
            desc = Translator.GetString("card_crap_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);

            frequency = 0;
        }
        protected cCrap(cCrap other) : base(other) { }
        public override object Clone() => new cCrap(this);
    }
}
