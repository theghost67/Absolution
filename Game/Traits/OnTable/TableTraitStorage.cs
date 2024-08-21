using System.Collections.Generic;

namespace Game.Traits
{
    // NOTE: store only structs to avoid same references on cloned objects
    /// <summary>
    /// Представляет список с парами ключ-значение для хранения особых данных навыка стола.
    /// </summary>
    public class TableTraitStorage : Dictionary<string, object>
    {
        public TableTraitStorage(TableTraitStorage otherStorage) : this()
        {
            foreach (KeyValuePair<string, object> pair in otherStorage)
                Add(pair.Key, pair.Value);
        }
        public TableTraitStorage(TraitStorage dataStorage) : this()
        {
            foreach (KeyValuePair<string, object> pair in dataStorage)
                Add(pair.Key, pair.Value);
        }
        public TableTraitStorage() : base() { }
    }
}
