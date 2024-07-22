namespace Game.Cards
{
    public class cAnderson : FieldCard
    {
        public cAnderson() : base("anderson", "p become_human", "p become_machine")
        {
            name = "Лейтенант Андерсон";
            desc = "Один из первых лейтенантов своего подразделения. Вскоре после появления сраного андроида-напарника в его жизни, он" +
                   "потерял веру в технологии и покинул полицейский участок. Теперь его жизнь заключается в бесцельном шатании по барам...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cAnderson(cAnderson other) : base(other) { }
        public override object Clone() => new cAnderson(this);
    }
}
