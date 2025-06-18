namespace Game.Cards
{
    public class cStudent : FieldCard
    {
        public cStudent() : base("student", "scholar")
        {
            name = Translator.GetString("card_student_1");
            desc = Translator.GetString("card_student_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cStudent(cStudent other) : base(other) { }
        public override object Clone() => new cStudent(this);
    }
}
