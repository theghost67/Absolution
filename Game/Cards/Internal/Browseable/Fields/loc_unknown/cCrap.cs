﻿namespace Game.Cards
{
    public class cCrap : FieldCard
    {
        public cCrap() : base("crap")
        {
            name = "Говняшка";
            desc = "Омерзительные экскременты";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 0f;
        }
        protected cCrap(cCrap other) : base(other) { }
        public override object Clone() => new cCrap(this);
    }
}