namespace Game.Cards
{
    public class cCultist : FieldCard
    {
        public cCultist() : base("cultist", "cult")
        {
            name = Translator.GetString("card_cultist_1");
            desc = Translator.GetString("card_cultist_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cCultist(cCultist other) : base(other) { }
        public override object Clone() => new cCultist(this);
    }
}
