namespace Game.Cards
{
    public class cHysteric : FieldCard
    {
        public cHysteric() : base("hysteric", "ultrasonic_scream")
        {
            name = Translator.GetString("card_hysteric_1");
            desc = Translator.GetString("card_hysteric_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cHysteric(cHysteric other) : base(other) { }
        public override object Clone() => new cHysteric(this);
    }
}
