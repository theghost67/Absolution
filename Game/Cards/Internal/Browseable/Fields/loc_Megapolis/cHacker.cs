namespace Game.Cards
{
    public class cHacker : FieldCard
    {
        public cHacker() : base("hacker", "hack")
        {
            name = "Хацкер";
            desc = "413 430 43D 434 43E 43D";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cHacker(cHacker other) : base(other) { }
        public override object Clone() => new cHacker(this);
    }
}
