namespace Game.Cards
{
    public class cBerlin : FieldCard
    {
        public cBerlin() : base("berlin", "chao")
        {
            name = "Берлин";
            desc = "За названием известного города скрывается один из самых изобретательных грабителей современной истории (и уничтожителей столов). " +
                   "Стоит признать, что он чуть-чуть того... отбитый на голову. На самом деле, из-за генетического заболевания жить ему осталось недолго," +
                   "возможно, именно поэтому он решил совершить ограбление грандиозных масштабов. И, возможно, поэтому он так отчаянно хочет пожертвовать собой.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBerlin(cBerlin other) : base(other) { }
        public override object Clone() => new cBerlin(this);
    }
}
