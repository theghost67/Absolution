namespace Game.Cards
{
    public class cDeath : FieldCard
    {
        public cDeath() : base("death", "execution")
        {
            name = Translator.GetString("card_death_1");
            desc = Translator.GetString("card_death_2");

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 5);

            frequency = 0;
        }
        protected cDeath(cDeath other) : base(other) { }
        public override object Clone() => new cDeath(this);
    }
}
