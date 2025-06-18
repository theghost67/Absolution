namespace Game.Cards
{
    public class cMaxwell : FieldCard
    {
        public cMaxwell() : base("maxwell", "mind_split", "dont_starve")
        {
            name = Translator.GetString("card_maxwell_1");
            desc = Translator.GetString("card_maxwell_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cMaxwell(cMaxwell other) : base(other) { }
        public override object Clone() => new cMaxwell(this);
    }
}
