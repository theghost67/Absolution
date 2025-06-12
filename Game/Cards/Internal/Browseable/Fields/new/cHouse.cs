namespace Game.Cards
{
    public class cHouse : FieldCard
    {
        public cHouse() : base("house", "cane", "insight", "doctor")
        {
            name = "Доктор Хаус";
            desc = "Доктор Грегори Хаус известен своими феноменальными способами лечить людей. Несмотря на ненависть к своим пациентам, " +
                   "у него неплохо выходит. Уилсон назвал бы его \"человеком тяжелой судьбы\", а его коллеги и вовсе бы сказали, что он сумасшедший. " +
                   "О, и ещё одна деталь: обладая титановой тростью, он вполне может и покалечить, если понадобится. Или если его выбесить.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHouse(cHouse other) : base(other) { }
        public override object Clone() => new cHouse(this);
    }
}
