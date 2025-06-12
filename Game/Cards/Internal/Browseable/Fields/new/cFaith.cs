namespace Game.Cards
{
    public class cFaith : FieldCard
    {
        public cFaith() : base("faith", "parkour", "runner")
        {
            name = "Фэйт";
            desc = "Чрезвычайно выносливая сокрушительница крыш высоток, не дающая покоя полштату полиции в городе. И она не просто прыгает - " +
                   "её побег от копов сопровождается самыми невероятными акробатическими кульбитами, во время исполнения которых по ней просто невозможно попасть!" +
                   "Шеф полиции высказывается о ней следующим образом: \"Да пристрелите её уже кто-нибудь, сколько можно пончики из участка воровать!\"";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cFaith(cFaith other) : base(other) { }
        public override object Clone() => new cFaith(this);
    }
}
