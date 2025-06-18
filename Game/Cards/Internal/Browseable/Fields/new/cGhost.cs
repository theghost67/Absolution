namespace Game.Cards
{
    public class cGhost : FieldCard
    {
        public cGhost() : base("ghost", "boo", "not_here", "creators_mark")
        {
            name = Translator.GetString("card_ghost_1");
            desc = Translator.GetString("card_ghost_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cGhost(cGhost other) : base(other) { }
        public override object Clone() => new cGhost(this);
    }
}
