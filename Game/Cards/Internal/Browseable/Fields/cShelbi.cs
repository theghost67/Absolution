namespace Game.Cards
{
    public class cShelbi : FieldCard
    {
        public cShelbi() : base("shelbi", "origami_victim", "origami_killer")
        {
            name = "Детектив Шелби";
            desc = "Мастерский детектив, тайно расследующий дело о Мастере Оригами. Говорят, убийца топит своих жертв в дождевой воде. " + 
                   "Так жестоко. И низко. Убийцей точно не может оказаться наш добрячок.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cShelbi(cShelbi other) : base(other) { }
        public override object Clone() => new cShelbi(this);
    }
}
