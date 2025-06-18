namespace Game.Cards
{
    public class cGazikel : FieldCard
    {
        public cGazikel() : base("gazikel", "vaccianide", "doctor")
        {
            name = Translator.GetString("card_gazikel_1");
            desc = Translator.GetString("card_gazikel_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cGazikel(cGazikel other) : base(other) { }
        public override object Clone() => new cGazikel(this);
    }
}
