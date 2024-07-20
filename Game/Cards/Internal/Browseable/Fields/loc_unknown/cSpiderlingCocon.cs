namespace Game.Cards
{
    public class cSpiderlingCocon : FieldCard
    {
        public cSpiderlingCocon() : base("spiderling_cocon")
        {
            name = "Паучий кокон";
            desc = "Кокон";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0;
        }
        protected cSpiderlingCocon(cSpiderlingCocon other) : base(other) { }
        public override object Clone() => new cSpiderlingCocon(this);
    }
}
