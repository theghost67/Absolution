namespace Game.Cards
{
    public class cFaith : FieldCard
    {
        public cFaith() : base("faith", "parkour", "runner")
        {
            name = Translator.GetString("card_faith_1");
            desc = Translator.GetString("card_faith_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cFaith(cFaith other) : base(other) { }
        public override object Clone() => new cFaith(this);
    }
}
