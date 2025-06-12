namespace Game.Cards
{
    public class cAlex : FieldCard
    {
        public cAlex() : base("alex", "finger")
        {
            name = "Алекс";
            desc = "Лев, который любит насмехаться над всем и вся. Серьёзно, он вас просто загнобит при первой встрече. " +
                   "А ещё я слышал, что однажды он своих друзей чуть не слопал! Зоопарк какой-то...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cAlex(cAlex other) : base(other) { }
        public override object Clone() => new cAlex(this);
    }
}
