namespace Game.Cards
{
    public class cBrick : FieldCard
    {
        public cBrick() : base("brick", "bricky_taste")
        {
            name = Translator.GetString("card_brick_1");
            desc = Translator.GetString("card_brick_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBrick(cBrick other) : base(other) { }
        public override object Clone() => new cBrick(this);
    }
}
