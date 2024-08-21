namespace Game.Cards
{
    public class cDoof : FieldCard
    {
        public cDoof() : base("doof", "doofinator 2", "self_destruction")
        {
            name = "Фуфел";
            desc = "Множество опытов, экспериментов и изобретений помогли обрести Фуфелшмерцу славу по всему штату. " +
                   "К сожалению, он прославился тем, что его изобретения всегда проваливались, как бы он ни старался над ними. И ещё этот утконос...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cDoof(cDoof other) : base(other) { }
        public override object Clone() => new cDoof(this);
    }
}
