namespace Game.Cards
{
    public class cProfessor : FieldCard
    {
        public cProfessor() : base("professor", "planned", "tactician")
        {
            name = Translator.GetString("card_professor_1");
            desc = Translator.GetString("card_professor_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cProfessor(cProfessor other) : base(other) { }
        public override object Clone() => new cProfessor(this);
    }
}
