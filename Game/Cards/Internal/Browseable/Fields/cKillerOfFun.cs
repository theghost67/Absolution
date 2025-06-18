namespace Game.Cards
{
    public class cKillerOfFun : FieldCard
    {
        public cKillerOfFun() : base("killer_of_fun", "nerf_time")
        {
            name = Translator.GetString("card_killer_of_fun_1");
            desc = Translator.GetString("card_killer_of_fun_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cKillerOfFun(cKillerOfFun other) : base(other) { }
        public override object Clone() => new cKillerOfFun(this);
    }
}
