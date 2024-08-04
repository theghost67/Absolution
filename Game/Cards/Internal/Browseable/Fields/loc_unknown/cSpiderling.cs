namespace Game.Cards
{
    public class cSpiderling : FieldCard
    {
        public cSpiderling() : base("spiderling", "weaver")
        {
            name = "Паукообразный";
            desc = "Разумный прямоходящий паук, питающийся мясом монстров и обречённый скитаться в одиночестве. Он пытался подружиться с кем-то из соседей, " +
                     "но они оказались настоящими свиньями и даже не впустили его в дом. Единственные друзья, которые у него есть - паучьи коконы.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cSpiderling(cSpiderling other) : base(other) { }
        public override object Clone() => new cSpiderling(this);
    }
}
