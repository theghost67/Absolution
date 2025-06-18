namespace Game.Cards
{
    public class cShadowheart : FieldCard
    {
        public cShadowheart() : base("shadowheart", "dark_ball", "dark_shield", "dark_plans")
        {
            name = Translator.GetString("card_shadowheart_1");
            desc = Translator.GetString("card_shadowheart_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cShadowheart(cShadowheart other) : base(other) { }
        public override object Clone() => new cShadowheart(this);
    }
}
