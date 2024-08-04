namespace Game.Cards
{
    public class cJeweller : FieldCard
    {
        public cJeweller() : base("jeweller", "cook")
        {
            name = "Ювелир";
            desc = "Властелин за... увеличительным стеклом";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cJeweller(cJeweller other) : base(other) { }
        public override object Clone() => new cJeweller(this);
    }
}
