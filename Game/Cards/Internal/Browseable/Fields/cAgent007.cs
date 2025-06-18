namespace Game.Cards
{
    public class cAgent007 : FieldCard
    {
        public cAgent007() : base("agent_007", "crosseyed_shooter")
        {
            name = Translator.GetString("card_agent007_1");
            desc = Translator.GetString("card_agent007_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cAgent007(cAgent007 other) : base(other) { }
        public override object Clone() => new cAgent007(this);
    }
}
