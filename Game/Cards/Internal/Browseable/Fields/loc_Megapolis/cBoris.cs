﻿namespace Game.Cards
{
    public class cBoris : FieldCard
    {
        public cBoris() : base("boris") // "cat_secret"
        {
            name = "Кот Борис";
            desc = "";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cBoris(cBoris other) : base(other) { }
        public override object Clone() => new cBoris(this);
    }
}
