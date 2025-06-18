namespace Game.Cards
{
    public class cRein : FieldCard
    {
        public cRein() : base("rein", "hammer_go", "screen_shield")
        {
            name = Translator.GetString("card_rein_1");
            desc = Translator.GetString("card_rein_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cRein(cRein other) : base(other) { }
        public override object Clone() => new cRein(this);
    }
}
