namespace Game.Cards
{
    public class cBook : FieldCard
    {
        public cBook() : base("book", "") // TODO: add "story" passive trait
        {
            name = Translator.GetString("card_book_1");
            desc = Translator.GetString("card_book_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBook(cBook other) : base(other) { }
        public override object Clone() => new cBook(this);
    }
}
