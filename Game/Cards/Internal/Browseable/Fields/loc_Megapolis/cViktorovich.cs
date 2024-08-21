namespace Game.Cards
{
    public class cViktorovich : FieldCard
    {
        public cViktorovich() : base("viktorovich")
        {
            name = "Викторович";
            desc = "Машинист-ломатель";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cViktorovich(cViktorovich other) : base(other) { }
        public override object Clone() => new cViktorovich(this);
    }
}
