namespace Game.Cards
{
    public class cStanley : FieldCard
    {
        public cStanley() : base("stanley", "my_story", "sprinter")
        {
            name = Translator.GetString("card_stanley_1");
            desc = Translator.GetString("card_stanley_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cStanley(cStanley other) : base(other) { }
        public override object Clone() => new cStanley(this);
    }
}
