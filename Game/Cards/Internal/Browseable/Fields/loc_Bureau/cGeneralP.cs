namespace Game.Cards
{
    public class cGeneralP : FieldCard
    {
        public cGeneralP() : base("general_p", "order_of_attack", "order_of_defence", "tactician")
        {
            name = "Генерал П";
            desc = "Матёрый воитель, участвующий во множестве исторических сражений, а тажке переживший времена Великой Сегрегации и Эпохи Вавулизации. " + 
                   "Его тактический ум способен просчитывать исход битвы за десятки ходов вперёд. Однако, мало кто одобряет его радикальные приказы, " +
                   "зачастую приводящие в гарантированному взаимоуничтожению.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 1.00f;
        }
        protected cGeneralP(cGeneralP other) : base(other) { }
        public override object Clone() => new cGeneralP(this);
    }
}
