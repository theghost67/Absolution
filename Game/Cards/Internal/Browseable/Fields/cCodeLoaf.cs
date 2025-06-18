namespace Game.Cards
{
    public class cCodeLoaf : FieldCard
    {
        public cCodeLoaf() : base("code_loaf")
        {
            name = Translator.GetString("card_code_loaf_1");
            desc = Translator.GetString("card_code_loaf_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);

            frequency = 0;
        }
        protected cCodeLoaf(cCodeLoaf other) : base(other) { }
        public override object Clone() => new cCodeLoaf(this);
    }
}
