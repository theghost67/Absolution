﻿namespace Game.Cards
{
    public class cDj : FieldCard
    {
        public cDj() : base("dj", "wide_swing_plus")
        {
            name = "Ди-джей";
            desc = "Легендарный отжигатель, брейкдансер, ди-джей и просто дебил - он устраивает сотни тусовок, популярен по всей стране и знает все самые взрывные песни этого года. " +
                   "И он готов использовать их, чтобы навсегда уничтожить своих врагов. Уничтожить со стилем.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 4);
        }
        protected cDj(cDj other) : base(other) { }
        public override object Clone() => new cDj(this);
    }
}
