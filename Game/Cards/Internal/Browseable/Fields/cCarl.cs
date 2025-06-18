namespace Game.Cards
{
    public class cCarl : FieldCard
    {
        public cCarl() : base("carl", "reporting", "ministry_rat")
        {
            name = Translator.GetString("card_carl_1");
            desc = Translator.GetString("card_carl_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cCarl(cCarl other) : base(other) { }
        public override object Clone() => new cCarl(this);
    }
}
