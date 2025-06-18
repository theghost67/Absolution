namespace Game.Cards
{
    public class cHitman : FieldCard
    {
        public cHitman() : base("hitman", "scope", "stealth")
        {
            name = Translator.GetString("card_hitman_1");
            desc = Translator.GetString("card_hitman_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHitman(cHitman other) : base(other) { }
        public override object Clone() => new cHitman(this);
    }
}
