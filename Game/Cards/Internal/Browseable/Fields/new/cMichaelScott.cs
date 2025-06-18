namespace Game.Cards
{
    public class cMichaelScott : FieldCard
    {
        public cMichaelScott() : base("michael_scott", "parkour", "mikelove")
        {
            name = Translator.GetString("card_michael_scott_1");
            desc = Translator.GetString("card_michael_scott_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cMichaelScott(cMichaelScott other) : base(other) { }
        public override object Clone() => new cMichaelScott(this);
    }
}
