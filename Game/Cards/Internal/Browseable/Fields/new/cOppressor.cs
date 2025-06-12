namespace Game.Cards
{
    public class cOppressor : FieldCard
    {
        public cOppressor() : base("oppressor", "opressing")
        {
            name = "Угнетатель";
            desc = "Представьте себе такое понятие, как угнетение - подавленное состояние, при котором вам не хочется жить. " +
                   "Что ж, эта машинка является механическим воплощением данного термина. Не пройдёт и часа, как вы возненавидите эту летающую машину смерти. " +
                   "Она буквально не даст вам ЖИТЬ.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 4);
        }
        protected cOppressor(cOppressor other) : base(other) { }
        public override object Clone() => new cOppressor(this);
    }
}
