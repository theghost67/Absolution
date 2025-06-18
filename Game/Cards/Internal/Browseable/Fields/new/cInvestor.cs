namespace Game.Cards
{
    public class cInvestor : FieldCard
    {
        public cInvestor() : base("investor", "investment", "downside_bet")
        {
            name = Translator.GetString("card_investor_1");
            desc = Translator.GetString("card_investor_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cInvestor(cInvestor other) : base(other) { }
        public override object Clone() => new cInvestor(this);
    }
}
