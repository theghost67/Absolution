namespace Game.Cards
{
    public class cEntrepreneur : FieldCard
    {
        public cEntrepreneur() : base("entrepreneur", "hired")
        {
            name = Translator.GetString("card_entrepreneur_1");
            desc = Translator.GetString("card_entrepreneur_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cEntrepreneur(cEntrepreneur other) : base(other) { }
        public override object Clone() => new cEntrepreneur(this);
    }
}
