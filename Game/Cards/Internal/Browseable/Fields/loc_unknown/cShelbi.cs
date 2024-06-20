namespace Game.Cards
{
    public class cShelbi : FieldCard
    {
        public cShelbi() : base("shelbi", "p origami_killer")
        {
            name = "Детектив Шелби";
            desc = "Увлажняющий убийца";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 0.6f;
        }
        protected cShelbi(cShelbi other) : base(other) { }
        public override object Clone() => new cShelbi(this);
    }
}
