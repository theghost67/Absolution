namespace Game.Cards
{
    public class cCarl : FieldCard
    {
        public cCarl() : base("carl", "p rat")
        {
            name = "Карл-0";
            desc = "Министерская крыса";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 0.4f;
        }
        protected cCarl(cCarl other) : base(other) { }
        public override object Clone() => new cCarl(this);
        public override bool RangePotentialIsGuaranteed() => true;
    }
}
