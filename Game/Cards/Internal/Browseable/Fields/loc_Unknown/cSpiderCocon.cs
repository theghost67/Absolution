namespace Game.Cards
{
    public class cSpiderCocon : FieldCard
    {
        public cSpiderCocon() : base("spider_cocon")
        {
            name = "Паучий кокон";
            desc = "Смотри, там что-то шевелится. Не хочешь посмотреть что именно?";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cSpiderCocon(cSpiderCocon other) : base(other) { }
        public override object Clone() => new cSpiderCocon(this);
    }
}
