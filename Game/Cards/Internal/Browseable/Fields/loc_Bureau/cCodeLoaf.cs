namespace Game.Cards
{
    public class cCodeLoaf : FieldCard
    {
        public cCodeLoaf() : base("code_loaf", "summarizing")
        {
            name = "Спец-буханка";
            desc = "За тобой уже выехали!";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cCodeLoaf(cCodeLoaf other) : base(other) { }
        public override object Clone() => new cCodeLoaf(this);
    }
}
