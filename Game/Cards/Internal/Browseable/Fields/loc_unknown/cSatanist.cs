namespace Game.Cards
{
    public class cSatanist : FieldCard
    {
        public cSatanist() : base("satanist", "p sacrifice")
        {
            name = "Козёл-сатанист";
            desc = "Перед очередным жертвоприношением козы во имя всевышних сил, рядом стоящий козёл возмутился (откуда он там взялся - непонятно) " +
                     "и решил отомстить - теперь он приносит в жертву людей во славу сатане.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 0.4f;
        }
        protected cSatanist(cSatanist other) : base(other) { }
        public override object Clone() => new cSatanist(this);
    }
}
