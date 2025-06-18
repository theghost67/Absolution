namespace Game.Cards
{
    public class cArchivist : FieldCard
    {
        public cArchivist() : base("archivist", "search_in_archive")
        {
            name = Translator.GetString("card_archivist_1");
            desc = Translator.GetString("card_archivist_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cArchivist(cArchivist other) : base(other) { }
        public override object Clone() => new cArchivist(this);
    }
}
