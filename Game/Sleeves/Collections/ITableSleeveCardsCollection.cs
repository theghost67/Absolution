using System.Collections.Generic;

namespace Game.Sleeves
{
    /// <summary>
    /// Интерфейс, реализующий объект как коллекцию карт для рукава карт на столе (см. <see cref="TableSleeve"/>).
    /// </summary>
    public interface ITableSleeveCardsCollection : IReadOnlyList<ITableSleeveCard> 
    {
        public void Add(ITableSleeveCard card);
        public void Insert(ITableSleeveCard card, int index);
        public bool Remove(ITableSleeveCard card);
        public int IndexOf(ITableSleeveCard card);
        public void Clear();
    }
}
