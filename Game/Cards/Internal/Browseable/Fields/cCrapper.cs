namespace Game.Cards
{
    public class cCrapper : FieldCard
    {
        public cCrapper() : base("crapper", "smelly_trapper 4")
        {
            name = Translator.GetString("card_crapper_1");
            desc = Translator.GetString("card_crapper_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cCrapper(cCrapper other) : base(other) { }
        public override object Clone() => new cCrapper(this);
    }
}
