namespace Game.Cards
{
    public class cFinn : FieldCard
    {
        public cFinn() : base("finn", "furious_swing", "ooo")
        {
            name = Translator.GetString("card_finn_1");
            desc = Translator.GetString("card_finn_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cFinn(cFinn other) : base(other) { }
        public override object Clone() => new cFinn(this);
    }
}
