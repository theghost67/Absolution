namespace Game.Cards
{
    public class cWilson : FieldCard
    {
        public cWilson() : base("starve_wilson", "hardened", "dont_starve")
        {
            name = "Уилсон";
            desc = "Как-то раз Уилсон решил довериться Максвеллу, построив механизм невероятной сложности по его чертежам. Механизм заработал, и не спрашивая мнения Уилсона" +
                   "затянул его внутрь себя. Уилсон оказался в Постоянной. Долгие годы он был вынужден питаться мясом монстров и пережаренными ягодами, в полном одиночестве. " +
                   "Но это его только закалило.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cWilson(cWilson other) : base(other) { }
        public override object Clone() => new cWilson(this);
    }
}
