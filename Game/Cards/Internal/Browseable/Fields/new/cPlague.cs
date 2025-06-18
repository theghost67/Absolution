namespace Game.Cards
{
    public class cPlague : FieldCard
    {
        public cPlague() : base("plague", "poison_grenade", "weeds")
        {
            name = Translator.GetString("card_plague_1");
            desc = Translator.GetString("card_plague_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cPlague(cPlague other) : base(other) { }
        public override object Clone() => new cPlague(this);
    }
}
