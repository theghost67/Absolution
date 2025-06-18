namespace Game.Cards
{
    public class cMichaelSanta : FieldCard
    {
        public cMichaelSanta() : base("michael_santa", "grand_thief", "plunder", "mikelove")
        {
            name = Translator.GetString("card_michael_santa_1");
            desc = Translator.GetString("card_michael_santa_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cMichaelSanta(cMichaelSanta other) : base(other) { }
        public override object Clone() => new cMichaelSanta(this);
    }
}
