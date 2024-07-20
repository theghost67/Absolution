namespace Game.Cards
{
    public class cDoof : FieldCard
    {
        public cDoof() : base("doof")
        {
            name = "Фуфел";
            desc = "Множество опытов, экспериментов и изобретений помогли обрести Фуфелшмерцу славу по всему штату. " +
                   "К сожалению, он прославился тем, что его изобретения всегда проваливались, как бы он ни старался над ними. И ещё этот утконос...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 1.00f;
        }
        protected cDoof(cDoof other) : base(other) { }
        public override object Clone() => new cDoof(this);
    }
}
