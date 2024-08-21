namespace Game.Cards
{
    public class cChief : FieldCard
    {
        public cChief() : base("chief", "cook")
        {
            name = "Шев";
            desc = "Шеф-повар";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cChief(cChief other) : base(other) { }
        public override object Clone() => new cChief(this);
    }
}
