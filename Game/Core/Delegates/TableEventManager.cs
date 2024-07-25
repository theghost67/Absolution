using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий счётчик выполняемых в данный момент событий стола. Позволяет ожидать завершение всех событий.<br/>
    /// При добавлении, обязательно должно быть и удаление (после завершения события).
    /// </summary>
    public static class TableEventManager
    {
        static int _runningCount;
        public static void Add()
        {
            _runningCount++;
        }
        public static void Remove()
        {
            _runningCount--;
        }

        public static bool CanAwaitTableEvents()
        {
            return _runningCount != 0;
        }
        public static bool CanAwaitTableCardQueue()
        {
            return TableFieldCardDrawerQueue.IsAnyRunning;
        }
        public static bool CanAwaitInitiationQueue()
        {
            return BattleInitiationQueue.IsAnyRunning;
        }
        public static bool CanAwaitAnyEvents()
        {
            return CanAwaitTableEvents() || CanAwaitTableCardQueue() || CanAwaitInitiationQueue();
        }

        // NOTE: can await initiation queue inside of the table event
        // TODO: try use without TableEventManager Adds/Removes?
        /* EXAMPLE:
           trait.Territory.Initiations.Run();
           TableEventManager.Remove(); // TableEventManager.AwaitAnyEvents will still work because of initiations queue
           await trait.Territory.Initiations.Await();
           TableEventManager.Add(); // restore event await condition
        */

        public static async UniTask AwaitTableEvents()
        {
            // awaits currently running asynchronous events of type TableEventVoid
            while (CanAwaitTableEvents())
                await UniTask.Yield();
        }
        public static async UniTask AwaitInitiationQueue()
        {
            // awaits asynchronous events not related to TableEventVoid class (usually static)
            while (CanAwaitInitiationQueue())
                await UniTask.Yield();
        }
        public static async UniTask AwaitAnyEvents()
        {
            // awaits all asynchronous events
            while (CanAwaitAnyEvents())
                await UniTask.Yield();
        }
    }
}
