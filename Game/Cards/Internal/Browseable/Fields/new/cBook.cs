namespace Game.Cards
{
    public class cBook : FieldCard
    {
        public cBook() : base("book", "") // TODO: add "story" passive trait
        {
            name = "Книга";
            desc = "Пыльная книжка, найденная в каком-то полусгоревшем складу. Под слоем пепла скрывался фантастический роман о похождении " +
                   "одинокого врача во времена зомби-апокалипсиса. И как любой роман, он способен держать в напряжении многих читателей. Жаль, " +
                   "что его не издали... Погодите, тут есть подпись на сгоревшей обложке: -кель. Интересно, кто это?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBook(cBook other) : base(other) { }
        public override object Clone() => new cBook(this);
    }
}
