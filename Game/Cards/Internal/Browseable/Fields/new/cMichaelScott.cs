namespace Game.Cards
{
    public class cMichaelScott : FieldCard
    {
        public cMichaelScott() : base("michael_scott", "parkour", "mikelove")
        {
            name = "Майкл Скотт";
            desc = "Майкл Скотт является управляющим одного из филиалов бумажной компании Дандр Миффлин. Его уникальные методики переговоров, съёмки, ходьбы, " +
                   "документирования, соблазнения, полемики, паркура, танцев, жестикулирования и дыхания делают его незаменимым сотрудником компании. " +
                   "А в этой компании есть легендарный Дуайт Шрут. Или Шрют? Да какая разница! Чёрт, место для описания кончается... ПАРКУР!";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cMichaelScott(cMichaelScott other) : base(other) { }
        public override object Clone() => new cMichaelScott(this);
    }
}
