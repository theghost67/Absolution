namespace Game.Cards
{
    public class cGermanSausages : FieldCard
    {
        public cGermanSausages() : base("german_sausages", "food_poisoning", "meaty")
        {
            name = Translator.GetString("card_german_sausages_1");
            desc = Translator.GetString("card_german_sausages_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cGermanSausages(cGermanSausages other) : base(other) { }
        public override object Clone() => new cGermanSausages(this);
    }
}
