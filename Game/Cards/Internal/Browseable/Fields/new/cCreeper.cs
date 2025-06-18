namespace Game.Cards
{
    public class cCreeper : FieldCard
    {
        public cCreeper() : base("creeper", "boom")
        {
            name = Translator.GetString("card_creeper_1");
            desc = Translator.GetString("card_creeper_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cCreeper(cCreeper other) : base(other) { }
        public override object Clone() => new cCreeper(this);
    }
}
