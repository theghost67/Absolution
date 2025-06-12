namespace Game.Cards
{
    public class cBrick : FieldCard
    {
        public cBrick() : base("brick", "bricky_taste")
        {
            name = "Кирпич";
            desc = "Как же хочется сейчас перекусить свежевыпеченным кирпичом! А с соусом, ммм... Объедение... Что? Уже и пошутить нельзя? " +
                   "Ну, а что ещё можно сказать? Это просто кирпич! Можете с его помощью дом построить, либо об голову кому-нибудь разбить. Решайте сами.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBrick(cBrick other) : base(other) { }
        public override object Clone() => new cBrick(this);
    }
}
