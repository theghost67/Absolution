namespace Game.Cards
{
    public class cHouseWilson : FieldCard
    {
        public cHouseWilson() : base("house_wilson", "sad_news", "innocence", "doctor")
        {
            name = "Доктор Уилсон";
            desc = "Невинное и доброе создание, пытающееся всеми силами помогать как можно большему количеству людей. Возможно, поэтому он " +
                   "является излюбленной целью приколов со стороны Хауса. К сожалению, Уилсон так и не смог найти собственное счастье... " +
                   "Ему остаётся только прикалываться вместе с Хаусом над всем остальным штатом сотрудников. Пока Кадди и Форман не видят.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHouseWilson(cHouseWilson other) : base(other) { }
        public override object Clone() => new cHouseWilson(this);
    }
}
