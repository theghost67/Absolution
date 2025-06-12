namespace Game.Cards
{
    public class c626 : FieldCard
    {
        public c626() : base("626", "naga_kwista", "multipaw")
        {
            name = "Образец 626";
            desc = "Инопланетный образец, полученный в результате экспериментов злого гения. Этот малыш способен поднимать вес в 3000 раз больше себя, " +
                   "съедать целые автобусы и харкать кислотой на расстояние до 100 метров! Был способен. Сейчас он послушный пёсик одной Гавайской девочки...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected c626(c626 other) : base(other) { }
        public override object Clone() => new c626(this);
    }
}
