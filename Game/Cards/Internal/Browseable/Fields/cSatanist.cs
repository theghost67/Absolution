namespace Game.Cards
{
    public class cSatanist : FieldCard
    {
        public cSatanist() : base("satanist", "sacrifice")
        {
            name = Translator.GetString("card_satanist_1");
            desc = Translator.GetString("card_satanist_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cSatanist(cSatanist other) : base(other) { }
        public override object Clone() => new cSatanist(this);
    }
}
