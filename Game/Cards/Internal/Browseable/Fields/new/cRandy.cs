namespace Game.Cards
{
    public class cRandy : FieldCard
    {
        public cRandy() : base("randy", "random")
        {
            name = "Рэнди отбитый";
            desc = "ЭЙ! КТО ЕГО ВЫПУСТИЛ!? НЕУЖЕЛИ ВЫ ХОТИТЕ, ЧТОБЫ ЭТОТ БЕЗУМНЫЙ РАССКАЗЧИК ВЫНЕС ВАМ МОЗГ СВОЕЙ СЛУЧАЙНОСТЬЮ!? " +
                   "Что ж, удачи вам в попытках контролировать его хаотичную натуру...";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cRandy(cRandy other) : base(other) { }
        public override object Clone() => new cRandy(this);
    }
}
