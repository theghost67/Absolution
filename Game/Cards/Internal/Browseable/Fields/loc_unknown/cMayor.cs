namespace Game.Cards
{
    public class cMayor : FieldCard
    {
        public cMayor() : base("mayor")
        {
            name = "Мэр";
            desc = "Представитель от народа";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1f;
        }
        protected cMayor(cMayor other) : base(other) { }
        public override object Clone() => new cMayor(this);
    }
}
