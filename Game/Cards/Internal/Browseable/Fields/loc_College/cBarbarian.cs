namespace Game.Cards
{
    public class cBarbarian : FieldCard
    {
        public cBarbarian() : base("barbarian", "unscheduled_test", "boiling_anger")
        {
            name = "Варварка";
            desc = "Когда ученики приходят на очередной урок, который ведёт Варвара, они понимают - сейчас они будут страдать.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBarbarian(cBarbarian other) : base(other) { }
        public override object Clone() => new cBarbarian(this);
    }
}
