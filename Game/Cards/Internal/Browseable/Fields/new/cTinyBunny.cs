namespace Game.Cards
{
    public class cTinyBunny : FieldCard
    {
        public cTinyBunny() : base("tiny_bunny", "feed", "innocence")
        {
            name = Translator.GetString("card_tiny_bunny_1");
            desc = Translator.GetString("card_tiny_bunny_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cTinyBunny(cTinyBunny other) : base(other) { }
        public override object Clone() => new cTinyBunny(this);
    }
}
