namespace Game.Cards
{
    public class cWardrobe : FieldCard
    {
        public cWardrobe() : base("wardrobe", "furniture_protection")
        {
            name = Translator.GetString("card_wardrobe_1");
            desc = Translator.GetString("card_wardrobe_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cWardrobe(cWardrobe other) : base(other) { }
        public override object Clone() => new cWardrobe(this);
    }
}
