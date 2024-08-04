namespace Game.Cards
{
    public class cIncredible : FieldCard
    {
        public cIncredible() : base("incredible", "incredible")
        {
            name = "Мистер Исключительный";
            desc = "Исключительный герой";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cIncredible(cIncredible other) : base(other) { }
        public override object Clone() => new cIncredible(this);
    }
}
