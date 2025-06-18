namespace Game.Cards
{
    public class cCanteen : FieldCard
    {
        public cCanteen() : base("canteen", "lunch_break")
        {
            name = Translator.GetString("card_canteen_1");
            desc = Translator.GetString("card_canteen_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cCanteen(cCanteen other) : base(other) { }
        public override object Clone() => new cCanteen(this);
    }
}
