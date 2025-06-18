namespace Game.Cards
{
    public class cBerlin : FieldCard
    {
        public cBerlin() : base("berlin", "chao")
        {
            name = Translator.GetString("card_berlin_1");
            desc = Translator.GetString("card_berlin_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBerlin(cBerlin other) : base(other) { }
        public override object Clone() => new cBerlin(this);
    }
}
