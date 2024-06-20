using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tScholar : PassiveTrait
    {
        const string ID = "scholar";

        public tScholar() : base(ID)
        {
            name = "Ученик";
            desc = "Изучает различные темы и вопросы.";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tScholar(tScholar other) : base(other) { }
        public override object Clone() => new tScholar(this);
    }
}
