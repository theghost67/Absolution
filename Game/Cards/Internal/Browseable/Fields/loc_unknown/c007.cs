namespace Game.Cards
{
    public class c007 : FieldCard
    {
        public c007() : base("007")
        {
            name = "Агент 0.07";
            desc = "Бывший игрок с врождённым косоглазием. Среднее соотношение его игровых убийств к смертям составляло 0.07. После множества жалоб, " +
                   "власти забрали его в свой участок и выяснили, что в реальной жизни его стрельба необычайно эффективна. Теперь он работает. Шпионом.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0.8f;
        }
        protected c007(c007 other) : base(other) { }
        public override object Clone() => new c007(this);
    }
}
