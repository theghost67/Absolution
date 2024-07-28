namespace Game.Cards
{
    public class cMichael : FieldCard
    {
        public cMichael() : base("michael", "zen_school", "scholar")
        {
            name = "Майкл";
            desc = "Один из студентов, который спустя годы учёбы и унижений со стороны " +
                   "Каленского принял свою нелёгкую судьбу и познал дзен, открывший ему способность в передаче духовной энергии.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cMichael(cMichael other) : base(other) { }
        public override object Clone() => new cMichael(this);
    }
}
