namespace Game.Cards
{
    public class cGazikel : FieldCard
    {
        public cGazikel() : base("gazikel", "vaccianide", "doctor")
        {
            name = "Газикель";
            desc = "Он просто сумасшдедший. Я серьёзно! Всего того, что он делает за один только день будет достаточно, чтобы отправиться на электрический стул. " +
                   "Представьте самые бесчеловечные эксперименты, которые можете представить. А теперь представьте, что бывают эксперименты ещё более отвратительные. " +
                   "Обладая мистической неуловимостью, ему до сих пор удаётся оставаться на свободе. Остановите его, кто-нибудь...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cGazikel(cGazikel other) : base(other) { }
        public override object Clone() => new cGazikel(this);
    }
}
