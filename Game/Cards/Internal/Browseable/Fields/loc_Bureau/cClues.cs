namespace Game.Cards
{
    public class cClues : FieldCard
    {
        public cClues() : base("clues", "a summarizing")
        {
            name = "Улики";
            desc = "Совершенно секретная информация, которая используется против вас. Любое ваше действие, указанное в этих бумагах, " +
                   "будет достаточным аргументом, чтобы заставить вас замолчать навсегда. Возможно, стоит избавиться от этих улик?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 1.00f;
        }
        protected cClues(cClues other) : base(other) { }
        public override object Clone() => new cClues(this);
    }
}
