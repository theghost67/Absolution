namespace Game.Cards
{
    public class cQueen : FieldCard
    {
        public cQueen() : base("queen", "chess")
        {
            name = Translator.GetString("card_queen_1");
            desc = Translator.GetString("card_queen_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cQueen(cQueen other) : base(other) { }
        public override object Clone() => new cQueen(this);
    }
}
