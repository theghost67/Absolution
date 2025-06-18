namespace Game.Cards
{
    public class cMoshev : FieldCard
    {
        public cMoshev() : base("moshev", "armored_tank", "meaty", "tactician")
        {
            name = Translator.GetString("card_moshev_1");
            desc = Translator.GetString("card_moshev_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cMoshev(cMoshev other) : base(other) { }
        public override object Clone() => new cMoshev(this);
    }
}
