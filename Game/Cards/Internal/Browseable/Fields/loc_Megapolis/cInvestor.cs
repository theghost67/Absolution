namespace Game.Cards
{
    public class cInvestor : FieldCard
    {
        public cInvestor() : base("investor")
        {
            name = "Инвестор";
            desc = "Утилизатор бабла";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cInvestor(cInvestor other) : base(other) { }
        public override object Clone() => new cInvestor(this);
    }
}
