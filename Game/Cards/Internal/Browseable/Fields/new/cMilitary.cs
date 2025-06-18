namespace Game.Cards
{
    public class cMilitary : FieldCard
    {
        public cMilitary() : base("military", "we_need_you")
        {
            name = Translator.GetString("card_military_1");
            desc = Translator.GetString("card_military_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);

            frequency = 0.5f;
        }
        protected cMilitary(cMilitary other) : base(other) { }
        public override object Clone() => new cMilitary(this);
    }
}
