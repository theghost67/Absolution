namespace Game.Cards
{
    public class cShadowheart : FieldCard
    {
        public cShadowheart() : base("shadowheart", "dark_ball", "dark_shield", "dark_plans")
        {
            name = "Шэдоу";
            desc = "Её сердце давно стало лишь тенью того, что было на его месте раньше. Ей <u>абсолют</u>но плевать на то, что правильно, а что нет - " +
                   "у неё есть задача, и она выполнит свою задачу. Чёртова бездушная машина... Ты просто бездушная машина смерти!";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cShadowheart(cShadowheart other) : base(other) { }
        public override object Clone() => new cShadowheart(this);
    }
}
