namespace Game.Cards
{
    public class cBarbarian : FieldCard
    {
        public cBarbarian() : base("barbarian", "a unscheduled_test", "p boiling_anger")
        {
            name = "Варварка";
            desc = "Когда ученики приходят на очередной урок, который ведёт Варвара, они понимают - сейчас они будут страдать.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cBarbarian(cBarbarian other) : base(other) { }
        public override object Clone() => new cBarbarian(this);
    }
}
