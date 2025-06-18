using System;

namespace Game.Cards
{
    public class cJoy : FieldCard
    {
        static readonly double _points = Math.Round(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds);
        public cJoy() : base("joy", "robbery", "creators_mark")
        {
            name = Translator.GetString("card_joy_1");
            desc = Translator.GetString("card_joy_2", _points);


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cJoy(cJoy other) : base(other) { }
        public override object Clone() => new cJoy(this);
    }
}
