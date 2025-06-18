namespace Game.Cards
{
    public class cPigeon : FieldCard
    {
        public cPigeon() : base("pigeon", "pigeon_fright", "white_bombing", "creators_mark")
        {
            name = Translator.GetString("card_pigeon_1");
            desc = Translator.GetString("card_pigeon_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cPigeon(cPigeon other) : base(other) { }
        public override object Clone() => new cPigeon(this);
    }
}
