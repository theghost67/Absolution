namespace Game.Cards
{
    public class cJake : FieldCard
    {
        public cJake() : base("jake", "party_animal", "mutated")
        {
            name = "Джейк";
            desc = "Джейк-псина- ой, то есть Джейк-пёс повидал множество различных тварей и живёт более, чем спокойной жизнью. Вероятно, из-за того, что " +
                   "он может растягиваться как ненормальный, что делает его неубиваемым ни от ран, ни от болезней. Жаль, что он слишком ленив, " +
                   "чтобы использовать свои силы на полную. В последнее время он просто шатается по клубам и отжигает на танцполе.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cJake(cJake other) : base(other) { }
        public override object Clone() => new cJake(this);
    }
}
