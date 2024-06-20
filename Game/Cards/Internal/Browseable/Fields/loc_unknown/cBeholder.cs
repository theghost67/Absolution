﻿namespace Game.Cards
{
    public class cBeholder : FieldCard
    {
        public cBeholder() : base("beholder")
        {
            name = "Агент Наблюдателя";
            desc = "Тот, кто всегда следит. Тот, кто всегда на шаг впереди. Тот, кто уже наставил свой ствол в вашу сторону - Наблюдатель. " +
                   "Агенты этой организации обладают даром убеждения и заставляют усомниться в каждом, кого вы знаете.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 0.10f;
        }
        protected cBeholder(cBeholder other) : base(other) { }
        public override object Clone() => new cBeholder(this);
    }
}