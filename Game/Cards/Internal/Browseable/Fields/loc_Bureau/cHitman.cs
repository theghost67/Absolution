namespace Game.Cards
{
    public class cHitman : FieldCard
    {
        public cHitman() : base("hitman", "scope", "stealth")
        {
            name = "Хитман";
            desc = "Всю жизнь выполнял контракты и расстреливал свои цели направо и налево. Никто не знает, " +
                   "сколько человек пали от его пули, но, без сомнений, свои контракты он выполнит, чего бы это не стоило.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cHitman(cHitman other) : base(other) { }
        public override object Clone() => new cHitman(this);

        public override bool RangePotentialIsGuaranteed() => true;
    }
}
