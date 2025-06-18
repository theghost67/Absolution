namespace Game.Cards
{
    public class cRandy : FieldCard
    {
        public cRandy() : base("randy", "random")
        {
            name = Translator.GetString("card_randy_1");
            desc = Translator.GetString("card_randy_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cRandy(cRandy other) : base(other) { }
        public override object Clone() => new cRandy(this);
    }
}
