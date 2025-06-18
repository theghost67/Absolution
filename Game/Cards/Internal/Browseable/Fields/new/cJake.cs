namespace Game.Cards
{
    public class cJake : FieldCard
    {
        public cJake() : base("jake", "party_animal", "mutated")
        {
            name = Translator.GetString("card_jake_1");
            desc = Translator.GetString("card_jake_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cJake(cJake other) : base(other) { }
        public override object Clone() => new cJake(this);
    }
}
