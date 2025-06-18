namespace Game.Cards
{
    public class cAlex : FieldCard
    {
        public cAlex() : base("alex", "finger")
        {
            name = Translator.GetString("card_alex_1");
            desc = Translator.GetString("card_alex_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cAlex(cAlex other) : base(other) { }
        public override object Clone() => new cAlex(this);
    }
}
