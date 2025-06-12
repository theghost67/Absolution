namespace Game.Cards
{
    public class cMongol : FieldCard
    {
        public cMongol() : base("mongol", "brawl")
        {
            name = "Пьяный монгол";
            desc = "Этого монгола сторонятся почти все барные заведения. Как только он оказывается в подобном, все точно знают - " +
                   "скоро от бара останутся только щепки и голый фундамент. Как-то раз он даже привёл медведя в бар и устроил с ним поножовщину, во история была!";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cMongol(cMongol other) : base(other) { }
        public override object Clone() => new cMongol(this);
    }
}
