﻿namespace Game.Cards
{
    public class cSecretary : FieldCard
    {
        public cSecretary() : base("secretary", "p rat")
        {
            name = "Секретарша";
            desc = "Офисная крыса";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cSecretary(cSecretary other) : base(other) { }
        public override object Clone() => new cSecretary(this);

        public override bool RangePotentialIsGuaranteed() => true;
    }
}
