﻿namespace Game.Cards
{
    public class cDallas : FieldCard
    {
        public cDallas() : base("dallas", "robbery")
        {
            name = "Даллас";
            desc = "Будете проходить мимо банка и увидете мужика в маске возле мусорных баков, знайте - это Даллас. Пакует очередную партию охранников." +
                   "Несмотря на первое впечатление, Даллас является одним из самых опытных грабителей на этом свете. Он способен выносить миллионы как долларов, так и тазеров." +
                   "Но перед этим потребуется немало перезапусков...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cDallas(cDallas other) : base(other) { }
        public override object Clone() => new cDallas(this);
    }
}