namespace Game.Cards
{
    public class cPigeon : FieldCard
    {
        public cPigeon() : base("pigeon", "pigeon_fright", "white_bombing", "creators_mark")
        {
            name = "Голубь";
            desc = "Этот голубь прожил большую часть своей жизни в городской среде. Имеет в запасе множество сюрпризов, " +
                   "готовых свалиться на голову.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cPigeon(cPigeon other) : base(other) { }
        public override object Clone() => new cPigeon(this);
    }
}
