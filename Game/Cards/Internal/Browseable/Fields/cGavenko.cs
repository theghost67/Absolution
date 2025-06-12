namespace Game.Cards
{
    public class cGavenko : FieldCard
    {
        public cGavenko() : base("gavenko", "testing", "old_authority")
        {
            name = "Гавенко";
            desc = "Всемирно известный тестировщик ПО, изобрёвший такие термины, как: вёкщит, висио, " +
                   "распараливание и ретон, которые используются в современном мире IT практически ежедневно.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cGavenko(cGavenko other) : base(other) { }
        public override object Clone() => new cGavenko(this);
    }
}
