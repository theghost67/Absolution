namespace Game.Cards
{
    public class cAgentOfBeholder : FieldCard
    {
        public cAgentOfBeholder() : base("agent_of_beholder", "recruitment")
        {
            name = Translator.GetString("card_agent_of_beholder_1");
            desc = Translator.GetString("card_agent_of_beholder_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cAgentOfBeholder(cAgentOfBeholder other) : base(other) { }
        public override object Clone() => new cAgentOfBeholder(this);
    }
}
