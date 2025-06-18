namespace Game.Cards
{
    public class cConnor : FieldCard
    {
        public cConnor() : base("connor", "time_to_decide")
        {
            name = Translator.GetString("card_connor_1");
            desc = Translator.GetString("card_connor_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cConnor(cConnor other) : base(other) { }
        public override object Clone() => new cConnor(this);
    }
}
