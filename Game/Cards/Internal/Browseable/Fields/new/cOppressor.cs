namespace Game.Cards
{
    public class cOppressor : FieldCard
    {
        public cOppressor() : base("oppressor", "opressing")
        {
            name = Translator.GetString("card_oppressor_1");
            desc = Translator.GetString("card_oppressor_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 4);
        }
        protected cOppressor(cOppressor other) : base(other) { }
        public override object Clone() => new cOppressor(this);
    }
}
