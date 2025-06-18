namespace Game.Cards
{
    public class cChief : FieldCard
    {
        public cChief() : base("chief", "fry")
        {
            name = Translator.GetString("card_chief_1");
            desc = Translator.GetString("card_chief_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cChief(cChief other) : base(other) { }
        public override object Clone() => new cChief(this);
    }
}
