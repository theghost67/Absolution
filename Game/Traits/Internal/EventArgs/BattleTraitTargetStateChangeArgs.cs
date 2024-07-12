using Game.Cards;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для функций, вызываемых при изменении состояния целей навыков сражения.
    /// </summary>
    public class BattleTraitTargetStateChangeArgs
    {
        public readonly IBattleTrait trait;
        public readonly BattleFieldCard target;
        public readonly bool canSeeTarget;

        public BattleTraitTargetStateChangeArgs(IBattleTrait trait, BattleFieldCard target, bool state)
        {
            this.trait = trait;
            this.target = target;
            this.canSeeTarget = state;
        }
    }
}
