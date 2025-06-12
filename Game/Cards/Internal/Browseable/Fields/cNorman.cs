﻿namespace Game.Cards
{
    public class cNorman : FieldCard
    {
        public cNorman() : base("norman", "triptocainum", "ari_record")
        {
            name = "Норман Джейден";
            desc = "Норман Джейден, ФБР. Инновационные очки с технологией УРС позволяют ему анализировать улики прямо на месте. " +
                   "К сожалению, у использования очков есть побочные эффекты. Небольшие. Избежать их позволяет особое вещество - Триптокаинум. " +
                   "Вероятно, после его принятия побочных эффектов ещё больше...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cNorman(cNorman other) : base(other) { }
        public override object Clone() => new cNorman(this);
    }
}
