namespace Game.Cards
{
    public class cArchivist : FieldCard
    {
        public cArchivist() : base("archivist")
        {
            name = "Сотрудник Архива";
            desc = "";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1f;
        }
        protected cArchivist(cArchivist other) : base(other) { }
        public override object Clone() => new cArchivist(this);
    }
}
