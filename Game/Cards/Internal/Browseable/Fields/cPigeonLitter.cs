namespace Game.Cards
{
    public class cPigeonLitter : FieldCard
    {
        public cPigeonLitter() : base("pigeon_litter")
        {
            name = Translator.GetString("card_pigeon_litter_1");
            desc = Translator.GetString("card_pigeon_litter_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);

            frequency = 0;
        }
        protected cPigeonLitter(cPigeonLitter other) : base(other) { }
        public override object Clone() => new cPigeonLitter(this);
    }
}
