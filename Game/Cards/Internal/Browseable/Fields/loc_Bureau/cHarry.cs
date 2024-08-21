﻿namespace Game.Cards
{
    public class cHarry : FieldCard
    {
        public cHarry() : base("harry", "alco_heal", "alco_rage")
        {
            name = "Детектив Гарри";
            desc = "Гарри использует своё алкогольное озарение для раскрытия самых запутанных дел. Удивительно, но это действительно работает. " +
                   "К сожалению, такой образ жизни заставил его отказаться от семьи и высокого положения. И от печени.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cHarry(cHarry other) : base(other) { }
        public override object Clone() => new cHarry(this);
    }
}
