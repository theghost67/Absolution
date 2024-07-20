namespace Game.Cards
{
    public class cPeter : FieldCard
    {
        public cPeter() : base("peter")
        {
            name = "Питер Паркер";
            desc = "Фотограф";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cPeter(cPeter other) : base(other) { }
        public override object Clone() => new cPeter(this);
    }
}
