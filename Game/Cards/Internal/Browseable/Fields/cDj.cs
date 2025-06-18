namespace Game.Cards
{
    public class cDj : FieldCard
    {
        public cDj() : base("dj", "wide_swing_plus")
        {
            name = Translator.GetString("card_dj_1");
            desc = Translator.GetString("card_dj_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 4);
        }
        protected cDj(cDj other) : base(other) { }
        public override object Clone() => new cDj(this);
    }
}
