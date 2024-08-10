using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Threading;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий счётчик выполняемых в данный момент событий стола. Позволяет ожидать завершение всех событий.<br/>
    /// При добавлении, обязательно должно быть и удаление (после завершения события).
    /// </summary>
    public static class TableEventManager
    {
        public static bool debug;
        static readonly HashSet<int> _set = new();

        static float _targetTimeIncrease = 8f;
        static float _targetTime = _targetTimeIncrease;
        static float _time;

        static TableEventManager() 
        {
            Global.OnFixedUpdate += OnFixedUpdate;
        }
        static void OnFixedUpdate()
        {
            if (!debug) return;
            _time += UnityEngine.Time.fixedDeltaTime;
            if (_time < _targetTime) return;
            _targetTime += _targetTimeIncrease;
            if (_set.Count == 0)
            {
                UnityEngine.Debug.Log($"ADD/REM: CLEAR, COUNT: {Count()}");
                return;
            }
            string str = "";
            foreach (int id in _set)
                str += $"{id}, ";
            UnityEngine.Debug.LogError($"ADD/REM MISMATCH, COUNT: {Count()}, PAIR NOT FOUND FOR NEXT IDS:\n{str}");
        }

        public static void Add(int id)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (currentThreadId != PlayerLoopHelper.MainThreadId)
                return;

            _set.Add(id);
            if (debug)
                UnityEngine.Debug.Log($"ADD ID: {id}, COUNT: {Count()}");
        }
        public static void Remove(int id)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (currentThreadId != PlayerLoopHelper.MainThreadId)
                return;

            if (!debug)
                _set.Remove(id);
            else if (!_set.Remove(id))
                 UnityEngine.Debug.LogError($"REM ID NOT FOUND: {id}, COUNT: {Count()}");
            else UnityEngine.Debug.Log($"REM ID: {id}, COUNT: {Count()}");
        }
        public static int Count() => _set.Count;

        public static bool CanAwaitTableEvents()
        {
            return _set.Count > 0;
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
            return (CanAwaitTableEvents() || CanAwaitTableCardQueue() || CanAwaitInitiationQueue());
        }

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
