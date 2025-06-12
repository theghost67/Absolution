namespace Game.Cards
{
    public class cCrap : FieldCard
    {
        public cCrap() : base("crap")
        {
            name = "Говняшка";
            desc = "Психически тяжёлая для обезвреживания ловушка.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);

            frequency = 0;
        }
        protected cCrap(cCrap other) : base(other) { }
        public override object Clone() => new cCrap(this);
    }
}
