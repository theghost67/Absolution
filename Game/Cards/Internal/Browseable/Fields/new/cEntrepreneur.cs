namespace Game.Cards
{
    public class cEntrepreneur : FieldCard
    {
        public cEntrepreneur() : base("entrepreneur", "hired")
        {
            name = "Предприниматель";
            desc = "Его уволили с десятой по счёту работы, и он решил открыть свой бизнес. Ну, если подпольную продажу рабов можно назвать бизнесом. " +
                   "Теперь он скитается по разным неприметным подвалам со своей труппой, демонстрируя свой эксклюзивный товар доверенным лицам... Да уж... " +
                   "Надеюсь, его лавочку рано или поздно прикроют, как и его самого.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cEntrepreneur(cEntrepreneur other) : base(other) { }
        public override object Clone() => new cEntrepreneur(this);
    }
}
