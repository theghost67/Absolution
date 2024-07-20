namespace Game.Cards
{
    public class cHysteric : FieldCard
    {
        public cHysteric() : base("hysteric", "p ultrasonic_scream")
        {
            name = "Истеричка";
            desc = "Довольно раздражительна. Возможно слишком. А ещё может одним визгом разбить все стёкла в радиусе ста метров.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cHysteric(cHysteric other) : base(other) { }
        public override object Clone() => new cHysteric(this);
    }
}
