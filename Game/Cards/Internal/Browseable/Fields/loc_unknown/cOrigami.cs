namespace Game.Cards
{
    public class cOrigami : FieldCard
    {
        public cOrigami() : base("origami")
        {
            name = "Оригами";
            desc = "Улика";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0;
        }
        protected cOrigami(cOrigami other) : base(other) { }
        public override object Clone() => new cOrigami(this);
    }
}
