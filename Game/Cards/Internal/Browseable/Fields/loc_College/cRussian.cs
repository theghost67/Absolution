namespace Game.Cards
{
    public class cRussian : FieldCard
    {
        public cRussian() : base("russian", "p look_of_despair")
        {
            name = "Россиянин";
            desc = "Насмотревшись на серость, окружающую его на протяжении всей жизни, он сам стал серым и унылым. " +
                   "Большинство просто избегают зрительного контакта с ним или делают вид, что его нет, лишь бы не стать таким же.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
            frequency = 0.75f;
        }
        protected cRussian(cRussian other) : base(other) { }
        public override object Clone() => new cRussian(this);
    }
}
