using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Представляет список с парами ключ-значение для хранения особых данных навыка.
    /// </summary>
    public class TraitStorage : Dictionary<string, object>
    {
        public const int TURNS_DELAY = -1;
        public const int TURNS_PASSED = -2;

        public readonly Trait trait;
        public int turnsDelay;
        public int turnsPassed;

        public TraitStorage(Trait trait, TraitStorage otherStorage) : this(trait, otherStorage.turnsDelay, otherStorage.turnsPassed)
        {
            foreach (KeyValuePair<string, object> pair in otherStorage)
                Add(pair.Key, pair.Value);
        }
        public TraitStorage(Trait trait, int turnsDelay = 0, int turnsPassed = 0) : base()
        {
            this.trait = trait;
            this.turnsDelay = turnsDelay;
            this.turnsPassed = turnsPassed;
        }
    }
}
