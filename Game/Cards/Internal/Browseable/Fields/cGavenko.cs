namespace Game.Cards
{
    public class cGavenko : FieldCard
    {
        public cGavenko() : base("gavenko", "testing", "old_authority")
        {
            name = Translator.GetString("card_gavenko_1");
            desc = Translator.GetString("card_gavenko_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cGavenko(cGavenko other) : base(other) { }
        public override object Clone() => new cGavenko(this);
    }
}
