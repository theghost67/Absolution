namespace Game.Cards
{
    public class cScp173 : FieldCard
    {
        public cScp173() : base("scp_173", "p fracture", "p fragile")
        {
            name = "Печенька";
            desc = "Разумная бетонная скульптура, перемещающаяся с высокой скоростью, когда её не видят. " +
                     "Крайне враждебен к каждому человеку и пытается [ДАННЫЕ УДАЛЕНЫ], ломая его шею. Лучше не проморгайте её.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 0);
            frequency = 0.3f;
        }
        protected cScp173(cScp173 other) : base(other) { }
        public override object Clone() => new cScp173(this);
    }
}
