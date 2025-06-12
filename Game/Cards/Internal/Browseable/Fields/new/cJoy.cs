using System;

namespace Game.Cards
{
    public class cJoy : FieldCard
    {
        static readonly double _points = Math.Round(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds);
        public cJoy() : base("joy", "robbery", "creators_mark")
        {
            name = "Джой";
            desc = $"Одержимая компьютерными играми грабительница, превратившая городской террор в аркадную забаву. Ведёт нескончаемый счёт очков, " +
                   $"которая она \"получает\" за устранение тазеров, бульдозеров и джеки-чанов. По-моему, она ещё и в сети его ведёт. Так, сейчас посмотрим..." +
                   $"О, нашёл! {_points} очков! Ну что, впечатляет?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cJoy(cJoy other) : base(other) { }
        public override object Clone() => new cJoy(this);
    }
}
