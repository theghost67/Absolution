namespace Game.Cards
{
    public class cPigeon : FieldCard
    {
        public cPigeon() : base("pigeon", "p pigeon_fright", "a white_bombing")
        {
            name = "Голубь";
            desc = "Этот голубь прожил большую часть своей жизни в городской среде. Имеет в запасе множество сюрпризов, " +
                   "готовых свалиться на голову.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
            frequency = 0.55f;
        }
        protected cPigeon(cPigeon other) : base(other) { }
        public override object Clone() => new cPigeon(this);
    }
}
