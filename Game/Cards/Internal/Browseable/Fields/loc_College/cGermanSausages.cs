namespace Game.Cards
{
    public class cGermanSausages : FieldCard
    {
        public cGermanSausages() : base("sausages", "food_poisoning")
        {
            name = "Немецкие сосиски";
            desc = "Дас ис гуд сасиски - сказал один из немцев, перед тем как засунуть вторую в рот. " +
                   "Выглядит довольно мерзко. Как долго эти сосиски валялись в холодильнике? " +
                   "Никто не знает. Возможно, если дать их кому-нибудь на пробу, он сможет ответить...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 1.00f;
        }
        protected cGermanSausages(cGermanSausages other) : base(other) { }
        public override object Clone() => new cGermanSausages(this);
    }
}
