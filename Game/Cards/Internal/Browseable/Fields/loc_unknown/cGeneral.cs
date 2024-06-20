namespace Game.Cards
{
    public class cGeneral : FieldCard
    {
        public cGeneral() : base("general_p")
        {
            name = "Генерал П";
            desc = "Двойник Ким-Чен-Ына";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 0.05f;
        }
        protected cGeneral(cGeneral other) : base(other) { }
        public override object Clone() => new cGeneral(this);
    }
}
