namespace Game.Cards
{
    public class cVavulov : FieldCard
    {
        public cVavulov() : base("vavulov", "competitive_obsession", "scholar", "mutated")
        {
            name = Translator.GetString("card_vavulov_1");
            desc = Translator.GetString("card_vavulov_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cVavulov(cVavulov other) : base(other) { }
        public override object Clone() => new cVavulov(this);
    }
}
