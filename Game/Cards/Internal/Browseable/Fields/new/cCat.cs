namespace Game.Cards
{
    public class cCat : FieldCard
    {
        public cCat() : base("cat", "nine_lives", "innocence")
        {
            name = Translator.GetString("card_cat_1");
            desc = Translator.GetString("card_cat_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cCat(cCat other) : base(other) { }
        public override object Clone() => new cCat(this);
    }
}
