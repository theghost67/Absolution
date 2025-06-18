namespace Game.Cards
{
    public class cHarry : FieldCard
    {
        public cHarry() : base("harry", "alco_heal", "alco_rage")
        {
            name = Translator.GetString("card_harry_1");
            desc = Translator.GetString("card_harry_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHarry(cHarry other) : base(other) { }
        public override object Clone() => new cHarry(this);
    }
}
