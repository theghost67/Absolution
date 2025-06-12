namespace Game.Cards
{
    public class cChief : FieldCard
    {
        public cChief() : base("chief", "fry")
        {
            name = "Шеф";
            desc = "Лучший повар по версии Мишчлен, сотворяющий свои шедевры в цитадели гастрономического удовольствия под названием Клод Моне. " +
                   "На кухне происходило множество самых разных эпизодов (а именно 120), за это время Шеф многое узнал, включая разные техники приготовления." +
                   "И если шеф пришёл на работу с бодуна, лучше не попадайтесь ему под руки - скорее всего, вы станете основным ингредиентом его приготовлений...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cChief(cChief other) : base(other) { }
        public override object Clone() => new cChief(this);
    }
}
