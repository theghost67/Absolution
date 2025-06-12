namespace Game.Cards
{
    public class cCardsharper : FieldCard
    {
        public cCardsharper() : base("cardsharper", "table_manipulations")
        {
            name = "Шулер";
            desc = "Пришёл как-то Балатро в казино. Сел за покерный стол, сделал ставку. Затем, вытащил из колоды все 52 карты, вытряхнул из-за своего рукава" +
                   "252 000 джокеров и сказал, что он победил. Начал, значит, дилер читать информацию, написанную на джокерах. Первый джокер, второй, третий, четвёртый, пятый..." +
                   "Дилер умер, не успев за свою короткую жизнь дочитать всех джокеров. Больше Балатро в это казино не пускали... Зато пустили в другое!";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cCardsharper(cCardsharper other) : base(other) { }
        public override object Clone() => new cCardsharper(this);
    }
}
