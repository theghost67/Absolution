using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Класс, представляющий коллекцию асинхронных событий.<br/>
    /// Позволяет добавлять/удалять и ожидать завершения всех событий в данной группе.
    /// </summary>
    public class TableEventGroup : IEnumerable<int>
    {
        public readonly string id;
        private readonly HashSet<int> _set;

        public TableEventGroup(string id)
        { 
            this.id = id; 
            _set = new HashSet<int>();
        }

        public bool Add(int eventId)
        {
            return _set.Add(eventId);
        }
        public bool Remove(int eventId) 
        {
            return _set.Remove(eventId);
        }
        public int Count()
        {
            return _set.Count;
        }
        public async UniTask Await(int eventsThreshold = 0)
        {
            while (_set.Count > eventsThreshold)
                await UniTask.Yield();
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _set.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }
    }
}
