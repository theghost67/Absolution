namespace Game.Cards
{
    public class cWidow : FieldCard
    {
        public cWidow() : base("widow", "scope_plus", "shooting_passion")
        {
            name = Translator.GetString("card_widow_1");
            desc = Translator.GetString("card_widow_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cWidow(cWidow other) : base(other) { }
        public override object Clone() => new cWidow(this);
    }
}
