namespace Game.Cards
{
    public class cConsultant : FieldCard
    {
        public cConsultant() : base("consultant")
        {
            name = "Консультант";
            desc = "Вечно-улыбающаюся";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cConsultant(cConsultant other) : base(other) { }
        public override object Clone() => new cConsultant(this);
    }
}
