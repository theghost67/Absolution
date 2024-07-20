namespace Game.Cards
{
    public class cChief : FieldCard
    {
        public cChief() : base("chief", "p cook")
        {
            name = "Шев";
            desc = "Шеф-повар";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 1.00f;
        }
        protected cChief(cChief other) : base(other) { }
        public override object Clone() => new cChief(this);
    }
}
