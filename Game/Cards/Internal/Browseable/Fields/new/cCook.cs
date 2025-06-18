namespace Game.Cards
{
    public class cCook : FieldCard
    {
        public cCook() : base("cook", "sunrising_flame", "scorching_flame")
        {
            name = Translator.GetString("card_cook_1");
            desc = Translator.GetString("card_cook_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cCook(cCook other) : base(other) { }
        public override object Clone() => new cCook(this);
    }
}
