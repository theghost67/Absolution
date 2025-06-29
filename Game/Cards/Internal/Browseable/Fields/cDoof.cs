namespace Game.Cards
{
    public class cDoof : FieldCard
    {
        public cDoof() : base("doof", "doofinator", "self_destruction")
        {
            name = Translator.GetString("card_doof_1");
            desc = Translator.GetString("card_doof_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cDoof(cDoof other) : base(other) { }
        public override object Clone() => new cDoof(this);
    }
}
