namespace Game.Cards
{
    public class cGranny : FieldCard
    {
        public cGranny() : base("granny", "granny_alliance", "old_authority")
        {
            name = "Бабуся";
            desc = "Основательница Синдиката Бабуль, где каждая бабуля следит за каждым в городе, что позволяет оперативно решать проблемы.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cGranny(cGranny other) : base(other) { }
        public override object Clone() => new cGranny(this);
    }
}
