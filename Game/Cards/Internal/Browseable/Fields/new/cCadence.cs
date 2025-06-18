namespace Game.Cards
{
    public class cCadence : FieldCard
    {
        public cCadence() : base("cadence", "death_chord", "evasion")
        {
            name = Translator.GetString("card_cadence_1");
            desc = Translator.GetString("card_cadence_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cCadence(cCadence other) : base(other) { }
        public override object Clone() => new cCadence(this);
    }
}
