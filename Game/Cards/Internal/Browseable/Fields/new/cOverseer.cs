namespace Game.Cards
{
    public class cOverseer : FieldCard
    {
        public cOverseer() : base("overseer", "look")
        {
            name = Translator.GetString("card_overseer_1");
            desc = Translator.GetString("card_overseer_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cOverseer(cOverseer other) : base(other) { }
        public override object Clone() => new cOverseer(this);
    }
}
