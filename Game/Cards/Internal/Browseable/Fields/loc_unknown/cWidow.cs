namespace Game.Cards
{
    public class cWidow : FieldCard
    {
        public cWidow() : base("widow", "scope_plus", "shooting_passion")
        {
            name = "Вдова";
            desc = "Мастерский наёмный убийца, готовый уничтожить свою цель любой ценой. Даже ценой поражения в этом матче - неважно. " +
                   "Она знает как расставлять приоритеты. А ещё знает две фразы на английском: One shot. One kill.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 1.00f;
        }
        protected cWidow(cWidow other) : base(other) { }
        public override object Clone() => new cWidow(this);
    }
}
