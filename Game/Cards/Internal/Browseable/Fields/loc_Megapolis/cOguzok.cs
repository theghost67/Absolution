namespace Game.Cards
{
    public class cOguzok : FieldCard
    {
        public cOguzok() : base("oguzok", "cook")
        {
            name = "Огузок";
            desc = "Недо-повар";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cOguzok(cOguzok other) : base(other) { }
        public override object Clone() => new cOguzok(this);
    }
}
