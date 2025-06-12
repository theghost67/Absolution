namespace Game.Cards
{
    public class cWardrobe : FieldCard
    {
        public cWardrobe() : base("wardrobe", "furniture_protection")
        {
            name = "Шкаф";
            desc = "Абсолютно лучшее средство защиты против любых угроз. Этот шкаф, походу, сделали из вибраниума, иначе невозможно понять, " +
                   "как он вообще способен выдерживать удары такой космической силы. Поняв свою силу, шкаф обрёл самосознание и " +
                   "сделал своим долгом защищать всё, что ему дорого (трусы на нижней полке).";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cWardrobe(cWardrobe other) : base(other) { }
        public override object Clone() => new cWardrobe(this);
    }
}
