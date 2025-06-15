using Game.Territories;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Класс, представляющий словарь объектов типа &lt;<see cref="string"/>, <see cref="TableEntry"/>&gt;.
    /// </summary>
    public class TableEntryDict : Dictionary<string, TableEntry>, ITableEntryDict
    {
        public TableEntry Last => _last;
        TableEntry _last;

        public TableEntryDict() : base() { }
        public TableEntryDict(int capacity) : base(capacity) { }
        public TableEntryDict(IReadOnlyDictionary<string, TableEntry> collection) : base(collection) { }

        protected TableEntryDict(TableEntryDict src, TableEntryDictCloneArgs args) : this()
        {
            if (args.terrCArgs != null)
                args.terrCArgs.OnTerritoryReady += terr => Clone_OnTerritoryReady(src, terr);
        }
        public object Clone(CloneArgs args)
        {
            if (args is TableEntryDictCloneArgs cArgs)
                 return new TableEntryDict(this, cArgs);
            else return null;
        }

        public new void Add(string id, TableEntry entry) => AddInternal(id ?? NewId(), entry);
        public void Add(TableEntry entry) => AddInternal(NewId(), entry);

        static string NewId() => "_" + Unique.NewGuidStr;
        void Clone_OnTerritoryReady(TableEntryDict src, TableTerritory terr)
        {
            foreach (KeyValuePair<string, TableEntry> pair in src)
            {
                TableEntry srcEntry = pair.Value;
                TableEntryCloneArgs entryCArgs = new((ITableEntrySource)srcEntry.source?.Finder.FindOnTable(terr));
                TableEntry entryClone = (TableEntry)srcEntry.Clone(entryCArgs);
                Add(pair.Key, entryClone);
            }
        }

        void AddInternal(string key, TableEntry value)
        {
            if (!ContainsKey(key))
            {
                _last = value;
                base.Add(key, value);
            }
            else UnityEngine.Debug.LogWarning($"TableEntryDict key already exists: {key}. Value: {value.value}, source: {value.source}.");
        }
    }
}
