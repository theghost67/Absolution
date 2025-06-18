namespace Game.Cards
{
    public class cHouse : FieldCard
    {
        public cHouse() : base("house", "cane", "insight", "doctor")
        {
            name = Translator.GetString("card_house_1");
            desc = Translator.GetString("card_house_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHouse(cHouse other) : base(other) { }
        public override object Clone() => new cHouse(this);
    }
}
