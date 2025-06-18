namespace Game.Cards
{
    public class cBread : FieldCard
    {
        public cBread() : base("bread", "bricky_taste", "food_poisoning")
        {
            name = Translator.GetString("card_bread_1");
            desc = Translator.GetString("card_bread_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBread(cBread other) : base(other) { }
        public override object Clone() => new cBread(this);
    }
}
