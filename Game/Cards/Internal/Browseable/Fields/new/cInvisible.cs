namespace Game.Cards
{
    public class cInvisible : FieldCard
    {
        public cInvisible() : base("invisible", "not_here")
        {
            name = Translator.GetString("card_invisible_1");
            desc = "";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 0);
        }
        protected cInvisible(cInvisible other) : base(other) { }
        public override object Clone() => new cInvisible(this);
    }
}
