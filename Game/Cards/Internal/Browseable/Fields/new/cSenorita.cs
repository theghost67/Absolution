namespace Game.Cards
{
    public class cSenorita : FieldCard
    {
        public cSenorita() : base("senorita", "builder")
        {
            name = "Сеньорита";
            desc = "Латиноамериканка, открывшая свой уникальный бизнес по строительству домов. Всё начинается с одного крошащегося кирпича, нежно помещаемого на землю. " +
                   "Ну, им всё и заканчивается, по правде говоря - дом завершён! Забыл упомянуть одну деталь - это бизнес по строительству домов для муравьёв. Удивительно, " +
                   "но это довольно прибыльный бизнес. Муравьи в качестве оплаты без проблем крадут кошельки незнакомцев из чужих домов. Огромная окупаемость! Ждём расширения бизнеса!";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cSenorita(cSenorita other) : base(other) { }
        public override object Clone() => new cSenorita(this);
    }
}
