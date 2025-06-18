namespace Game.Cards
{
    public class cHouseWilson : FieldCard
    {
        public cHouseWilson() : base("house_wilson", "sad_news", "innocence", "doctor")
        {
            name = Translator.GetString("card_house_wilson_1");
            desc = Translator.GetString("card_house_wilson_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHouseWilson(cHouseWilson other) : base(other) { }
        public override object Clone() => new cHouseWilson(this);
    }
}
