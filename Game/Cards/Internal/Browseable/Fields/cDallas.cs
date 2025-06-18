namespace Game.Cards
{
    public class cDallas : FieldCard
    {
        public cDallas() : base("dallas", "robbery", "its_guard")
        {
            name = Translator.GetString("card_dallas_1");
            desc = Translator.GetString("card_dallas_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cDallas(cDallas other) : base(other) { }
        public override object Clone() => new cDallas(this);
    }
}
