namespace Game.Cards
{
    public class cFengMine : FieldCard
    {
        public cFengMine() : base("feng_mine", "explosive_mine", "adrenaline", "creators_mark")
        {
            name = Translator.GetString("card_feng_mine_1");
            desc = Translator.GetString("card_feng_mine_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cFengMine(cFengMine other) : base(other) { }
        public override object Clone() => new cFengMine(this);
    }
}
