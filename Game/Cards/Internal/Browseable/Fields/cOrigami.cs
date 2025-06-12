namespace Game.Cards
{
    public class cOrigami : FieldCard
    {
        public cOrigami() : base("origami")
        {
            name = "Оригами";
            desc = "Улика, оставленная Мастером Оригами на теле своей жертвы. Значит ли это, что новая жертва Мастера уже на подходе?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cOrigami(cOrigami other) : base(other) { }
        public override object Clone() => new cOrigami(this);
    }
}
