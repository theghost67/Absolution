using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Представляет список с парами ключ-значение для хранения особых данных трейта стола.
    /// </summary>
    public class TableTraitStorage : Dictionary<int, object>
    {
        public const int TURNS_DELAY = -1;
        public const int TURNS_PASSED = -2;

        public readonly TableTrait trait;
        public int turnsDelay;  // aka cooldown
        public int turnsPassed; // since trait was added

        public TableTraitStorage(TableTrait trait, TableTraitStorage otherStorage) : this(trait, otherStorage.turnsDelay, otherStorage.turnsPassed)
        {
            foreach (KeyValuePair<int, object> pair in otherStorage)
                Add(pair.Key, pair.Value);
        }
        public TableTraitStorage(TableTrait trait, TraitStorage dataStorage) : this(trait, dataStorage.turnsDelay, dataStorage.turnsPassed)
        {
            foreach (KeyValuePair<int, object> pair in dataStorage)
                Add(pair.Key, pair.Value);
        }
        public TableTraitStorage(TableTrait trait, int turnsDelay = 0, int turnsPassed = 0) : base()
        {
            this.trait = trait;
            this.turnsDelay = turnsDelay;
            this.turnsPassed = turnsPassed;
        }
    }
}
