namespace Game.Cards
{
    public class cConnor : FieldCard
    {
        public cConnor() : base("connor")
        {
            name = "Коннор RK800";
            desc = "Новейшая модель андроида-расследователя, присланная из Киберлайф. Но кто же он на самом деле? " +
                   "Живое существо? Или просто машина? Просто машина. Ничего более.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1f;
        }
        protected cConnor(cConnor other) : base(other) { }
        public override object Clone() => new cConnor(this);
    }
}
