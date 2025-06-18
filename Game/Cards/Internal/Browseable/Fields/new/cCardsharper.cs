namespace Game.Cards
{
    public class cCardsharper : FieldCard
    {
        public cCardsharper() : base("cardsharper", "table_manipulations")
        {
            name = Translator.GetString("card_cardsharper_1");
            desc = Translator.GetString("card_cardsharper_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cCardsharper(cCardsharper other) : base(other) { }
        public override object Clone() => new cCardsharper(this);
    }
}
