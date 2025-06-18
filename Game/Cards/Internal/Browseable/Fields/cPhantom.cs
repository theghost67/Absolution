namespace Game.Cards
{
    public class cPhantom : FieldCard
    {
        public cPhantom() : base("phantom", "teleportation_bag 2", "deadly_crit")
        {
            name = Translator.GetString("card_phantom_1");
            desc = Translator.GetString("card_phantom_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cPhantom(cPhantom other) : base(other) { }
        public override object Clone() => new cPhantom(this);
    }
}
