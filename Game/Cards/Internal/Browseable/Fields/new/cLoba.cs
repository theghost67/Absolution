namespace Game.Cards
{
    public class cLoba : FieldCard
    {
        public cLoba() : base("loba", "mama_bag", "teleportation_bracelet", "creators_mark")
        {
            name = Translator.GetString("card_loba_1");
            desc = Translator.GetString("card_loba_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cLoba(cLoba other) : base(other) { }
        public override object Clone() => new cLoba(this);
    }
}
