namespace Game.Cards
{
    public class cCyberCutlet : FieldCard
    {
        public cCyberCutlet() : base("cyber_cutlet", "meaty", "competitive_obsession")
        {
            name = "Киберкотлета";
            desc = "Несколько лет исследований и опасных (и негуманных) экспериментов от учёных, " +
                   "и вот оно - человек и котлета в одном экземпляре. Чувствует себя одиноко в этом мире.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cCyberCutlet(cCyberCutlet other) : base(other) { }
        public override object Clone() => new cCyberCutlet(this);
    }
}
