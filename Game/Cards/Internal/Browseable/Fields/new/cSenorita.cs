namespace Game.Cards
{
    public class cSenorita : FieldCard
    {
        public cSenorita() : base("senorita", "builder")
        {
            name = Translator.GetString("card_senorita_1");
            desc = Translator.GetString("card_senorita_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cSenorita(cSenorita other) : base(other) { }
        public override object Clone() => new cSenorita(this);
    }
}
