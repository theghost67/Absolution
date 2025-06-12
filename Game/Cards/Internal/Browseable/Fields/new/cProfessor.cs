namespace Game.Cards
{
    public class cProfessor : FieldCard
    {
        public cProfessor() : base("professor", "planned", "tactician")
        {
            name = "Профессор";
            desc = "Скромный очкарик, проживающий в сыром заброшенном подвале и отдающий приказы своей команде словно марионеткам. У него есть планы на все случаи жизни. " +
                   "Умер член команды? Спланировано. Умер профессор? Спланировано. На банк скинули ядерную бомбу, которая уничтожила команду вместе с половиной населения? " +
                   "Спланировано. Ждёте пример, который не был спланирован профессором? Зря.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cProfessor(cProfessor other) : base(other) { }
        public override object Clone() => new cProfessor(this);
    }
}
