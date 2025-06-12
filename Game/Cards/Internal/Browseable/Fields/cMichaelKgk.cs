namespace Game.Cards
{
    public class cMichaelKgk : FieldCard
    {
        public cMichaelKgk() : base("michael_kgk", "zen_school", "scholar", "mikelove")
        {
            name = "Майкл";
            desc = "Один из студентов, который спустя годы учёбы и унижений со стороны " +
                   "Каленского принял свою нелёгкую судьбу и познал дзен, открывший ему способность в передаче духовной энергии.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cMichaelKgk(cMichaelKgk other) : base(other) { }
        public override object Clone() => new cMichaelKgk(this);
    }
}
