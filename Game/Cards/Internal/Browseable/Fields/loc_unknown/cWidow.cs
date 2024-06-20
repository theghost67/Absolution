namespace Game.Cards
{
    public class cWidow : FieldCard
    {
        public cWidow() : base("widow")
        {
            name = "Вдоводелка";
            desc = "Вань шот. Вань килль.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 0.25f;
        }
        protected cWidow(cWidow other) : base(other) { }
        public override object Clone() => new cWidow(this);
    }
}
