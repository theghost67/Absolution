namespace Game.Cards
{
    public class cSonic : FieldCard
    {
        public cSonic() : base("sonic", "gotta_go", "lightning_speed")
        {
            name = Translator.GetString("card_sonic_1");
            desc = Translator.GetString("card_sonic_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 0);
        }
        protected cSonic(cSonic other) : base(other) { }
        public override object Clone() => new cSonic(this);
    }
}
