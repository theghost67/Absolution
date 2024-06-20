namespace Game.Cards
{
    public class cTerrorist : FieldCard
    {
        public cTerrorist() : base("terrorist", "p on_ready")
        {
            name = "Террорист";
            desc = "Является частью террористической организации Пердящие микрофоны, чья цель - взорвать некие точки B. Никто так и не понял, что это означает, " +
                     "поэтому правительство вынуждено отправлять свой лучший спецназ в атакуемые террористами места, чтобы уничтожить их раз и навсегда.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 0.80f;
        }
        protected cTerrorist(cTerrorist other) : base(other) { }
        public override object Clone() => new cTerrorist(this);
    }
}
