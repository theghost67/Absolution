using Cysharp.Threading.Tasks;

namespace Game
{
    // NOTE: await WhenAll() is used when invoking initiation events
    /// <summary>
    /// Статический класс, представляющий счётчик выполняемых в данный момент событий стола. Позволяет ожидать завершение всех событий.<br/>
    /// При добавлении, обязательно должно быть и удаление (после завершения события).
    /// </summary>
    public static class TableEventManager
    {
        public static bool IsAnyRunning => _runningCount != 0;
        static int _runningCount;

        public static void Add()
        {
            _runningCount++;
        }
        public static void Remove()
        {
            _runningCount--;
        }

        public static async UniTask WhenAll()
        {
            while (_runningCount != 0)
                await UniTask.Yield();
        }
    }
}
