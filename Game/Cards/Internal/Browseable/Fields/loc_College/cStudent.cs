namespace Game.Cards
{
    public class cStudent : FieldCard
    {
        public cStudent() : base("student", "scholar")
        {
            name = "Студент";
            desc = "Прошло несколько лет с тех пор, как он поступил в так называемое ЧУПО КЖК. Он рассчитывал, что получит актуальные " +
                   "знания по его любимой профессии, но на самом деле он получил только разочарование. Печальная история.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cStudent(cStudent other) : base(other) { }
        public override object Clone() => new cStudent(this);
    }
}
