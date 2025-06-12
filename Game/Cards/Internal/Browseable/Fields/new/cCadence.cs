namespace Game.Cards
{
    public class cCadence : FieldCard
    {
        public cCadence() : base("cadence", "death_chord", "evasion")
        {
            name = "Каденс";
            desc = "Кто знал, что монстров можно уничтожать со стилем? Каденс жарит во всех смыслах! Её навыки игры на гитаре на столько невыносимы, " +
                   "что все монстры в её измерении поумирали. Но ей всё мало... Она хочет уничтожить ИХ ВСЕХ. Со стилем, конечно же. " +
                   "Хм, дежавю...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cCadence(cCadence other) : base(other) { }
        public override object Clone() => new cCadence(this);
    }
}
