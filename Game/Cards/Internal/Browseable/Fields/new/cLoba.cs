namespace Game.Cards
{
    public class cLoba : FieldCard
    {
        public cLoba() : base("loba", "mama_bag", "teleportation_bracelet", "creators_mark")
        {
            name = "Лоба";
            desc = "Эта мамочка предпочитает женский подход. Зачистка рынка это её ежедневное занятие. И зачищает она его по-разному. " +
                   "Иногда забирает все самые лучшие товары. А иногда забирает жизни самых лучших людей... Можете долго на неё не засматриваться, вы ей не интересны." +
                   "Если, конечно, у вас нет Крабера.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cLoba(cLoba other) : base(other) { }
        public override object Clone() => new cLoba(this);
    }
}
