namespace Game.Cards
{
    public class cMercy : FieldCard
    {
        public cMercy() : base("mercy", "healing_beam", "empowering_beam", "heroes_never_die")
        {
            name = "Мёрси";
            desc = "Карманный ангел-спаситель, способный создать из кого угодно машину смерти. Хотите безграничную мощь? Попробуйте карманного ангела. " +
                   "Защитите её от всяких безумцев с луками и катанами - и она ваша.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
            frequency = 1.00f;
        }
        protected cMercy(cMercy other) : base(other) { }
        public override object Clone() => new cMercy(this);
    }
}
