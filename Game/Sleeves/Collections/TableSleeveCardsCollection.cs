using System.Collections;
using System.Collections.Generic;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий коллекцию карт для рукава карт на столе (см. <see cref="TableSleeve"/>).
    /// </summary>
    public class TableSleeveCardsCollection : ITableSleeveCardsCollection
    {
        public int Count => _list.Count;
        readonly List<ITableSleeveCard> _list;

        public TableSleeveCardsCollection() { _list = new List<ITableSleeveCard>(); }
        public TableSleeveCardsCollection(int capacity) { _list = new List<ITableSleeveCard>(capacity); }
        public TableSleeveCardsCollection(IEnumerable<ITableSleeveCard> collection) { _list = new List<ITableSleeveCard>(collection); }

        public ITableSleeveCard this[int index] => _list[index];

        public void Add(ITableSleeveCard card) => _list.Add(card);
        public void Insert(ITableSleeveCard card, int index)
        {
            _list.Insert(index, card);
        }
        public bool Remove(ITableSleeveCard card) => _list.Remove(card);
        public int IndexOf(ITableSleeveCard card)
        {
            return _list.IndexOf(card);
        }
        public void Clear() => _list.Clear();

        public IEnumerator<ITableSleeveCard> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
