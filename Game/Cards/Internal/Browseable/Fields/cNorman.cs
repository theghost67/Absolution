namespace Game.Cards
{
    public class cNorman : FieldCard
    {
        public cNorman() : base("norman", "triptocainum", "ari_record")
        {
            name = Translator.GetString("card_norman_1");
            desc = Translator.GetString("card_norman_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cNorman(cNorman other) : base(other) { }
        public override object Clone() => new cNorman(this);
    }
}
