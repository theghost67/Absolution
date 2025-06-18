namespace Game.Cards
{
    public class cMaxwellClone : FieldCard
    {
        public cMaxwellClone() : base("maxwell_clone", "")
        {
            name = Translator.GetString("card_maxwell_clone_1");
            desc = Translator.GetString("card_maxwell_clone_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);

            frequency = 0;
        }
        protected cMaxwellClone(cMaxwellClone other) : base(other) { }
        public override object Clone() => new cMaxwellClone(this);
    }
}
