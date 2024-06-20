namespace Game.Cards
{
    public class cVanga : FieldCard
    {
        public cVanga() : base("vanga", "p prediction")
        {
            name = "Ванга";
            desc = "Недавно нанятый оперативник, обладающий экстрасенсорными способностями. Состоит в секретном проекте МК ВАНГУЛЬТРА." + 
                   "Отчёты проекта подтвердили получаемое тактическое преимущество во время боевых операций при участии Ванги.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0.45f;
        }
        protected cVanga(cVanga other) : base(other) { }
        public override object Clone() => new cVanga(this);
    }
}
