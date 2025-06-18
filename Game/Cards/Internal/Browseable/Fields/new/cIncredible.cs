namespace Game.Cards
{
    public class cIncredible : FieldCard
    {
        public cIncredible() : base("incredible", "exception", "exceptional")
        {
            name = Translator.GetString("card_incredible_1");
            desc = Translator.GetString("card_incredible_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cIncredible(cIncredible other) : base(other) { }
        public override object Clone() => new cIncredible(this);
    }
}
