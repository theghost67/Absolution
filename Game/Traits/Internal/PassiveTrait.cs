using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс для всех пассивных трейтов. Эти трейты имеют цели и активуются мгновенно.
    /// </summary>
    public abstract class PassiveTrait : Trait
    {
        public PassiveTrait(string id) : base(id, isPassive: true) { }
        protected PassiveTrait(PassiveTrait other) : base(other) { }
        public sealed override TableTrait CreateOnTable(Transform parent)
        {
            return new TablePassiveTrait(this, null, parent);
        }
    }
}
