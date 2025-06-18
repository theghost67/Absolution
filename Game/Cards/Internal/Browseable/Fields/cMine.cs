namespace Game.Cards
{
    public class cMine : FieldCard
    {
        public cMine() : base("mine", "explosive")
        {
            name = Translator.GetString("card_mine_1");
            desc = Translator.GetString("card_mine_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cMine(cMine other) : base(other) { }
        public override object Clone() => new cMine(this);
    }
}
