namespace Game.Cards
{
    public class c626 : FieldCard
    {
        public c626() : base("626", "naga_kwista", "multipaw")
        {
            name = Translator.GetString("card_626_1");
            desc = Translator.GetString("card_626_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected c626(c626 other) : base(other) { }
        public override object Clone() => new c626(this);
    }
}
