﻿namespace Game.Cards
{
    public class cCarl : FieldCard
    {
        public cCarl() : base("carl", "a reporting", "p ministry_rat")
        {
            name = "Карл-0";
            desc = "Карл Штейн, некогда занимающий должность смотрителя дома, решил окончательно перейти на сторону министерства," +
                   "предав всех, кого он знал. Не следует переступать ему дорогу, если только вы не хотите оказаться за решёткой.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cCarl(cCarl other) : base(other) { }
        public override object Clone() => new cCarl(this);
        public override bool RangePotentialIsGuaranteed() => true;
    }
}
