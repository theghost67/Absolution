﻿namespace Game.Cards
{
    public class cArchivist : FieldCard
    {
        public cArchivist() : base("archivist", "search_in_archive")
        {
            name = "Сотрудник Архива";
            desc = "Самый обыкновенный, ничем не примечательный сотрудник архива. Но это не значит, что он не сможет найти на вас компромат.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cArchivist(cArchivist other) : base(other) { }
        public override object Clone() => new cArchivist(this);
    }
}
