namespace Game.Cards
{
    public class cCarter : FieldCard
    {
        public cCarter() : base("carter", "shocking", "camper", "doctor")
        {
            name = Translator.GetString("card_carter_1");
            desc = Translator.GetString("card_carter_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cCarter(cCarter other) : base(other) { }
        public override object Clone() => new cCarter(this);
    }
}
