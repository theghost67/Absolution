namespace Game.Cards
{
    public class cGranny : FieldCard
    {
        public cGranny() : base("granny", "granny_alliance", "old_authority")
        {
            name = Translator.GetString("card_granny_1");
            desc = Translator.GetString("card_granny_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cGranny(cGranny other) : base(other) { }
        public override object Clone() => new cGranny(this);
    }
}
