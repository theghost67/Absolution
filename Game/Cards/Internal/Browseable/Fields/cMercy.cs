namespace Game.Cards
{
    public class cMercy : FieldCard
    {
        public cMercy() : base("mercy", "healing_beam", "empowering_beam", "heroes_never_die")
        {
            name = Translator.GetString("card_mercy_1");
            desc = Translator.GetString("card_mercy_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cMercy(cMercy other) : base(other) { }
        public override object Clone() => new cMercy(this);
    }
}
