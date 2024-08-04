namespace Game.Cards
{
    public class cClaudeMonet : FieldCard
    {
        public cClaudeMonet() : base("claude_monet", "maintenance")
        {
            name = "Клод Моне";
            desc = "Худший ресторан Москвы";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cClaudeMonet(cClaudeMonet other) : base(other) { }
        public override object Clone() => new cClaudeMonet(this);
    }
}
