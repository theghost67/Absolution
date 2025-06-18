namespace Game.Cards
{
    public class cSalad : FieldCard
    {
        public cSalad() : base("salad", "miracle_drug")
        {
            name = Translator.GetString("card_salad_1");
            desc = Translator.GetString("card_salad_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cSalad(cSalad other) : base(other) { }
        public override object Clone() => new cSalad(this);
    }
}
