namespace Game.Cards
{
    public class cBarbarian : FieldCard
    {
        public cBarbarian() : base("barbarian", "unscheduled_test", "boiling_anger")
        {
            name = Translator.GetString("card_barbarian_1");
            desc = Translator.GetString("card_barbarian_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBarbarian(cBarbarian other) : base(other) { }
        public override object Clone() => new cBarbarian(this);
    }
}
