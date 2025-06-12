namespace Game.Cards
{
    public class cMaxwellClone : FieldCard
    {
        public cMaxwellClone() : base("maxwell_clone", "")
        {
            name = "Тень Максвелла";
            desc = "Собирать. Рубить. Копать. Убивать.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);

            frequency = 0;
        }
        protected cMaxwellClone(cMaxwellClone other) : base(other) { }
        public override object Clone() => new cMaxwellClone(this);
    }
}
