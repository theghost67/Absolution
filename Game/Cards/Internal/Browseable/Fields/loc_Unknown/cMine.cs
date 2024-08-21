namespace Game.Cards
{
    public class cMine : FieldCard
    {
        public cMine() : base("mine", "explosive")
        {
            name = "Мина";
            desc = "Эй, тут кто-то мину отложил! Да нет, я имею в виду настоящую мину!";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cMine(cMine other) : base(other) { }
        public override object Clone() => new cMine(this);
    }
}
