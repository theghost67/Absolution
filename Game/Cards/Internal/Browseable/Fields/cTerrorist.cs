namespace Game.Cards
{
    public class cTerrorist : FieldCard
    {
        public cTerrorist() : base("terrorist", "on_lookout")
        {
            name = Translator.GetString("card_terrorist_1");
            desc = Translator.GetString("card_terrorist_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cTerrorist(cTerrorist other) : base(other) { }
        public override object Clone() => new cTerrorist(this);
    }
}
