namespace Game.Cards
{
    public class cBoris : FieldCard
    {
        public cBoris() : base("boris") // "p cat_secret"
        {
            name = "Кот Борис";
            desc = "";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 1.00f;
        }
        protected cBoris(cBoris other) : base(other) { }
        public override object Clone() => new cBoris(this);
    }
}
