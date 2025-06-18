namespace Game.Cards
{
    public class cScp173 : FieldCard
    {
        public cScp173() : base("scp_173", "turning_point")
        {
            name = Translator.GetString("card_scp173_1");
            desc = Translator.GetString("card_scp173_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cScp173(cScp173 other) : base(other) { }
        public override object Clone() => new cScp173(this);
    }
}
