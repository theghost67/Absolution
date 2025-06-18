namespace Game.Cards
{
    public class cRussian : FieldCard
    {
        public cRussian() : base("russian", "look_of_despair")
        {
            name = Translator.GetString("card_russian_1");
            desc = Translator.GetString("card_russian_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cRussian(cRussian other) : base(other) { }
        public override object Clone() => new cRussian(this);
    }
}
