namespace Game.Cards
{
    public class cBusinessman : FieldCard
    {
        public cBusinessman() : base("businessman")
        {
            name = "Бизнесмен";
            desc = "Знаете, он свого рода учёный.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1f;
        }
        protected cBusinessman(cBusinessman other) : base(other) { }
        public override object Clone() => new cBusinessman(this);
    }
}
