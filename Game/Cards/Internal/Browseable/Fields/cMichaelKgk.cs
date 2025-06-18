namespace Game.Cards
{
    public class cMichaelKgk : FieldCard
    {
        public cMichaelKgk() : base("michael_kgk", "zen_school", "scholar", "mikelove")
        {
            name = Translator.GetString("card_michael_kgk_1");
            desc = Translator.GetString("card_michael_kgk_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cMichaelKgk(cMichaelKgk other) : base(other) { }
        public override object Clone() => new cMichaelKgk(this);
    }
}
