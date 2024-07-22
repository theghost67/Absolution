namespace Game.Cards
{
    public class cAgent007 : FieldCard
    {
        public cAgent007() : base("agent_007", "p crosseyed_shooter")
        {
            name = "Агент 0.07";
            desc = "Бывший игрок с врождённым косоглазием. Среднее соотношение его игровых убийств к смертям составляло 0.07. После множества жалоб, " +
                   "власти забрали его в свой участок и выяснили, что в реальной жизни его стрельба необычайно эффективна. Теперь он работает. Шпионом.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cAgent007(cAgent007 other) : base(other) { }
        public override object Clone() => new cAgent007(this);
    }
}
