namespace Game.Cards
{
    public class cOrigami : FieldCard
    {
        public cOrigami() : base("origami")
        {
            name = Translator.GetString("card_origami_1");
            desc = Translator.GetString("card_origami_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cOrigami(cOrigami other) : base(other) { }
        public override object Clone() => new cOrigami(this);
    }
}
