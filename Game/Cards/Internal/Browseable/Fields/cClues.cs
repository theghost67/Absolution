namespace Game.Cards
{
    public class cClues : FieldCard
    {
        public cClues() : base("clues", "summarizing")
        {
            name = Translator.GetString("card_clues_1");
            desc = Translator.GetString("card_clues_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);

            frequency = 0;
        }
        protected cClues(cClues other) : base(other) { }
        public override object Clone() => new cClues(this);
    }
}
