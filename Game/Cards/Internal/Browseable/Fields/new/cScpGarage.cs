namespace Game.Cards
{
    public class cScpGarage : FieldCard
    {
        public cScpGarage() : base("scp_garage", "shift")
        {
            name = Translator.GetString("card_scp_garage_1");
            desc = Translator.GetString("card_scp_garage_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cScpGarage(cScpGarage other) : base(other) { }
        public override object Clone() => new cScpGarage(this);
    }
}
