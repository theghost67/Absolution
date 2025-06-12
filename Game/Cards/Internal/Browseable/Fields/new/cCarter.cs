namespace Game.Cards
{
    public class cCarter : FieldCard
    {
        public cCarter() : base("carter", "shocking", "camper", "doctor")
        {
            name = "Картер";
            desc = "В первый день своей работы Герман Картер вылечил своего пациента. Лечение началось со слов вроде \"КОМУ-ТО ТРЕБУЕТСЯ ШОКОВАЯ ТЕРАПИЯ, ХИ-ХИ-ХИ-ХИ!\" " +
                   "И действительно, он смог вылечить пациента от эпилепсии. Правда, он умер через пару секунд, как только его болезнь вылечили, но это не смущает Картера." +
                   "Лечение должно быть произведено любой ценой. Даже ценой СМЕРТИ.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cCarter(cCarter other) : base(other) { }
        public override object Clone() => new cCarter(this);
    }
}
