﻿namespace Game.Cards
{
    public class cPhantom : FieldCard
    {
        public cPhantom() : base("phantom")
        {
            name = "Фантомка";
            desc = "Множество лет скитаний по лесам научили её искуссным навыкам убийства, позволяя выдавать смертельные критические удары каждому, кто встанет у неё на пути. " +
                     "Но спустя какое-то время она задумалась о своих действиях, и решила перестать таким заниматься. В лесах.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 0.9f;
        }
        protected cPhantom(cPhantom other) : base(other) { }
        public override object Clone() => new cPhantom(this);
    }
}
