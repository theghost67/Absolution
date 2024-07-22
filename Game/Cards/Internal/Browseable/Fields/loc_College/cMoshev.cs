﻿namespace Game.Cards
{
    public class cMoshev : FieldCard
    {
        public cMoshev() : base("moshev", "p armored_tank", "p tactician")
        {
            name = "Мошев";
            desc = "То время, когда он хотел захватить человечество путём вавулизации, также известной как проект \"Вавулов-2\", запомнит каждый. " +
                   "После неудачной попытки порабощения, у него отобрали все инструменты и устройства. Странно, однако калаш у него никто не отбирал...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 1.00f;
        }
        protected cMoshev(cMoshev other) : base(other) { }
        public override object Clone() => new cMoshev(this);

        public override bool RangeSplashIsGuaranteed() => true;
    }
}
