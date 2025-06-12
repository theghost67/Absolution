namespace Game.Cards
{
    public class cLibrarian : FieldCard
    {
        public cLibrarian() : base("librarian", "ego")
        {
            name = "Библиотекарша";
            desc = "Не успели сдать книгу в библиотеку? Не волнуйтесь, данная особа примет ВАС в качестве книги на сдачу. За бесчисленное количество циклов, Анджела" +
                   "повидала много кошмарных аномалий. И ей уже абсолютно всё равно, что бы ни происходило, она готова совершить самые ужасные вещи для достижения своей цели." +
                   "Возможно, глубоко внутри её осталась капля человечности... Не, вряд ли.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 4);
        }
        protected cLibrarian(cLibrarian other) : base(other) { }
        public override object Clone() => new cLibrarian(this);
    }
}
