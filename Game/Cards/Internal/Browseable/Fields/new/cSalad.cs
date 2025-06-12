namespace Game.Cards
{
    public class cSalad : FieldCard
    {
        public cSalad() : base("salad", "miracle_drug")
        {
            name = "Салат";
            desc = "Ну наконец-то, самая обыкновенная человеческая еда! Как же я проголодался от всей этой работы описателя, знаете ли! " +
                   "В этом салате же нет никаких особых приправ, соусов или чего-то такого, верно?.. Верно?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cSalad(cSalad other) : base(other) { }
        public override object Clone() => new cSalad(this);
    }
}
