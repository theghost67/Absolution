namespace Game.Cards
{
    public class cMongol : FieldCard
    {
        public cMongol() : base("mongol", "brawl")
        {
            name = Translator.GetString("card_mongol_1");
            desc = Translator.GetString("card_mongol_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cMongol(cMongol other) : base(other) { }
        public override object Clone() => new cMongol(this);
    }
}
