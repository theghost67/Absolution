using System.Collections;
using System.Collections.Generic;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий коллекцию карт для рукава карт во время сражения (см. <see cref="BattleSleeve"/>).
    /// </summary>
    public class BattleSleeveCardsCollection : ITableSleeveCardsCollection, IReadOnlyList<IBattleSleeveCard>
    {
        public int Count => _list.Count;
        readonly List<IBattleSleeveCard> _list;

        public BattleSleeveCardsCollection() { _list = new List<IBattleSleeveCard>(); }
        public BattleSleeveCardsCollection(int capacity) { _list = new List<IBattleSleeveCard>(capacity); }
        public BattleSleeveCardsCollection(IEnumerable<IBattleSleeveCard> collection) { _list = new List<IBattleSleeveCard>(collection); }

        public IBattleSleeveCard this[int index] => _list[index];
        public void Add(IBattleSleeveCard card) => _list.Add(card);
        public bool Remove(IBattleSleeveCard card) => _list.Remove(card);
        public void Clear() => _list.Clear();

        ITableSleeveCard IReadOnlyList<ITableSleeveCard>.this[int index] => this[index];
        void ITableSleeveCardsCollection.Add(ITableSleeveCard card) => Add((IBattleSleeveCard)card);
        bool ITableSleeveCardsCollection.Remove(ITableSleeveCard card) => Remove((IBattleSleeveCard)card);

        public IEnumerator<IBattleSleeveCard> GetEnumerator() => _list.GetEnumerator();
        IEnumerator<ITableSleeveCard> IEnumerable<ITableSleeveCard>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
