namespace Game.Cards
{
    public class cBatman : FieldCard
    {
        public cBatman() : base("batman", "double_attack", "stealth")
        {
            name = "Ватмэн";
            desc = "Это не оригинальный Бэтмен, который кричит \"ГДЕ ДЕТОНАТОР!?\", летая туда-сюда по Готэм-сити, а всего лишь его жалкая пародия. " +
                   "Ватмен прыгает по крышам своего захолустья и следит, чтобы никто и никогда не выбрасывал мусор в неположенном месте. " +
                   "Странно, что его прозвище не ВатафакМэн...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBatman(cBatman other) : base(other) { }
        public override object Clone() => new cBatman(this);
    }
}
