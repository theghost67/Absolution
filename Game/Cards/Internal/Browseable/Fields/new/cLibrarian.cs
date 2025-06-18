namespace Game.Cards
{
    public class cLibrarian : FieldCard
    {
        public cLibrarian() : base("librarian", "ego")
        {
            name = Translator.GetString("card_librarian_1");
            desc = Translator.GetString("card_librarian_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 4);
        }
        protected cLibrarian(cLibrarian other) : base(other) { }
        public override object Clone() => new cLibrarian(this);
    }
}
