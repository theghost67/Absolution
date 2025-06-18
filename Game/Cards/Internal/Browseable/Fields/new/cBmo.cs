namespace Game.Cards
{
    public class cBmo : FieldCard
    {
        public cBmo() : base("bmo", "load", "ooo")
        {
            name = Translator.GetString("card_bmo_1");
            desc = Translator.GetString("card_bmo_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBmo(cBmo other) : base(other) { }
        public override object Clone() => new cBmo(this);
    }
}
