namespace Game.Cards
{
    public class cHobo : FieldCard
    {
        public cHobo() : base("hobo", "p brown_wave")
        {
            name = "Бомж";
            desc = "Король свалки";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cHobo(cHobo other) : base(other) { }
        public override object Clone() => new cHobo(this);
    }
}
