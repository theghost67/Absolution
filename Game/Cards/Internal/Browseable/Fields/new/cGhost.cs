namespace Game.Cards
{
    public class cGhost : FieldCard
    {
        public cGhost() : base("ghost", "boo", "not_here", "creators_mark")
        {
            name = "Призрак";
            desc = "Крайне редкое создание, обладающее необыкновенной любопытностью. Не обладая стареющим телом, он " +
                   "решил посвятить своё время на изучение мира и существ, обитающих в нём. Он побывал не просто в сотнях, " +
                   "а в тысячах мест, пробуя себя в новых ролях и смотря на развитие ситуаций со стороны.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cGhost(cGhost other) : base(other) { }
        public override object Clone() => new cGhost(this);
    }
}
