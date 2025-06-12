namespace Game.Cards
{
    public class cQueen : FieldCard
    {
        public cQueen() : base("queen", "chess")
        {
            name = "Королева";
            desc = "Королева чёрно-белого танцпола. Если шахматную доску можно назвать таковой. Эта мадам на первый взгляд безобидна, но как только вы оступитесь... " +
                   "Ох, готовьте защиту тыла... Рокируется вам в самое уязвимое место и засадит вам губительный шах и мат... Найдите ей короля, в конце концов.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cQueen(cQueen other) : base(other) { }
        public override object Clone() => new cQueen(this);
    }
}
