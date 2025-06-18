namespace Game.Cards
{
    public class cWilson : FieldCard
    {
        public cWilson() : base("starve_wilson", "hardened", "dont_starve")
        {
            name = Translator.GetString("card_wilson_1");
            desc = Translator.GetString("card_wilson_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cWilson(cWilson other) : base(other) { }
        public override object Clone() => new cWilson(this);
    }
}
