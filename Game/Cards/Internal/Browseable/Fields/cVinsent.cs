namespace Game.Cards
{
    public class cVinsent : FieldCard
    {
        public cVinsent() : base("vinsent", "way_out")
        {
            name = Translator.GetString("card_vinsent_1");
            desc = Translator.GetString("card_vinsent_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cVinsent(cVinsent other) : base(other) { }
        public override object Clone() => new cVinsent(this);
    }
}
