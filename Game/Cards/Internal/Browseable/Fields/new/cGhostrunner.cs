namespace Game.Cards
{
    public class cGhostrunner : FieldCard
    {
        public cGhostrunner() : base("ghostrunner", "hyper_reflex")
        {
            name = Translator.GetString("card_ghostrunner_1");
            desc = Translator.GetString("card_ghostrunner_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cGhostrunner(cGhostrunner other) : base(other) { }
        public override object Clone() => new cGhostrunner(this);
    }
}
