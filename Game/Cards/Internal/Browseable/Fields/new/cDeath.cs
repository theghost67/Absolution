namespace Game.Cards
{
    public class cDeath : FieldCard
    {
        public cDeath() : base("death", "execution")
        {
            name = "Смерть";
            desc = "Твоё приключение подошло к концу.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 5);

            frequency = 0;
        }
        protected cDeath(cDeath other) : base(other) { }
        public override object Clone() => new cDeath(this);
    }
}
