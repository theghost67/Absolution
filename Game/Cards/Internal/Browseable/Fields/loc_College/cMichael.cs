namespace Game.Cards
{
    public class cMichael : FieldCard
    {
        public cMichael() : base("michael", "a zen_school", "p scholar")
        {
            name = "Майкл";
            desc = "Один из студентов, который спустя годы учёбы и унижений со стороны " +
                   "Каленского принял свою нелёгкую судьбу и познал дзен, открывший ему способность в передаче духовной энергии.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 0.75f;
        }
        protected cMichael(cMichael other) : base(other) { }
        public override object Clone() => new cMichael(this);
    }
}
