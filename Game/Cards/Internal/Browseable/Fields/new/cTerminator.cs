namespace Game.Cards
{
    public class cTerminator : FieldCard
    {
        public cTerminator() : base("terminator", "hasta_la_vista", "scope", "exoskeleton")
        {
            name = Translator.GetString("card_terminator_1");
            desc = Translator.GetString("card_terminator_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cTerminator(cTerminator other) : base(other) { }
        public override object Clone() => new cTerminator(this);
    }
}
