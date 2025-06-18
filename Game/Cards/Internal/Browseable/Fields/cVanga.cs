namespace Game.Cards
{
    public class cVanga : FieldCard
    {
        public cVanga() : base("vanga", "prediction", "old_authority")
        {
            name = Translator.GetString("card_vanga_1");
            desc = Translator.GetString("card_vanga_2") + 
                   Translator.GetString("card_vanga_3");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cVanga(cVanga other) : base(other) { }
        public override object Clone() => new cVanga(this);
    }
}
