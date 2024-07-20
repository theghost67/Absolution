namespace Game.Cards
{
    public class cOtzdarva : FieldCard
    {
        public cOtzdarva() : base("otzdarva")
        {
            name = "Не Отздарва";
            desc = "Игрок соревновательных игр жанра хоррор, записывающий свои кульбиты и выкладывающий их на ютуб. Натточил своё мастерство настолько, " +
                     "что никакая падла не пройдёт мимо него. Но единственная эмоция, застывшая на нём - каменное лицо.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cOtzdarva(cOtzdarva other) : base(other) { }
        public override object Clone() => new cOtzdarva(this);
    }
}
