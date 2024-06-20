namespace Game.Cards
{
    public class cHarry : FieldCard
    {
        public cHarry() : base("harry")
        {
            name = "Детектив Гарри";
            desc = "Детектив Бухло Младший";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 0.90f;
        }
        protected cHarry(cHarry other) : base(other) { }
        public override object Clone() => new cHarry(this);
    }
}
