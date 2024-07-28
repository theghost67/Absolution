﻿namespace Game.Cards
{
    public class cCrapper : FieldCard
    {
        public cCrapper() : base("crapper", "brown_gift")
        {
            name = "Крэппер";
            desc = "Младший брат всеми известного убийцы, использующего медвежьи капканы. К сожалению, для него капканов не осталось. " +
                   "Но он придумал свою разновидность ловушек и расставляет их по всему миру. Эти ловушки тяжёло обезвредить... " +
                  "психически, поэтому лучше не пытаться - просто смотрите под ноги.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cCrapper(cCrapper other) : base(other) { }
        public override object Clone() => new cCrapper(this);
    }
}
