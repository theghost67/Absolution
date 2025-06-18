namespace Game.Cards
{
    public class cSans : FieldCard
    {
        public cSans() : base("sans", "bad_time", "special_attack")
        {
            name = Translator.GetString("card_sans_1");
            desc = Translator.GetString("card_sans_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cSans(cSans other) : base(other) { }
        public override object Clone() => new cSans(this);
    }
}
