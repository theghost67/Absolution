namespace Game.Cards
{
    public class cBarbarian : FieldCard
    {
        public cBarbarian() : base("barbarian", "a unscheduled_test", "p boiling_anger")
        {
            name = "Варварка";
            desc = "Когда ученики приходят на очередной урок, который ведёт Варвара, они понимают - сейчас они будут страдать.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 0.75f;
        }
        protected cBarbarian(cBarbarian other) : base(other) { }
        public override object Clone() => new cBarbarian(this);
    }
}
