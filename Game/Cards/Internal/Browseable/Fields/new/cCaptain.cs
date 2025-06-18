namespace Game.Cards
{
    public class cCaptain : FieldCard
    {
        public cCaptain() : base("captain", "hammer_out", "avenger")
        {
            name = Translator.GetString("card_captain_1");
            desc = Translator.GetString("card_captain_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cCaptain(cCaptain other) : base(other) { }
        public override object Clone() => new cCaptain(this);
    }
}
