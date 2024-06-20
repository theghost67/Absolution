namespace Game.Cards
{
    public class cClues : FieldCard
    {
        public cClues() : base("clues", "a clues_revealing")
        {
            name = "Разоблачающие улики";
            desc = "Совершенно секретные данные";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0.85f;
        }
        protected cClues(cClues other) : base(other) { }
        public override object Clone() => new cClues(this);
    }
}
