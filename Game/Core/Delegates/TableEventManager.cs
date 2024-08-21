using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий коллекцию выполняемых в данный момент асинхронных событий стола (т.е. тех, которые видит игрок).<br/>
    /// Позволяет ожидать завершения всех событий. При добавлении, обязательно должно быть и удаление (после завершения события).
    /// </summary>
    public static class TableEventManager
    {
        public static bool debug;
        static readonly List<TableEventGroup> _groups = new();
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
            _time += Time.fixedDeltaTime;
            if (_time < _targetTime) return;
            _targetTime += _targetTimeIncrease;
            foreach (TableEventGroup group in _groups)
            {
                if (group.Count() == 0)
                {
                    Debug.Log($"ADD/REM CLEAR // GROUP ID: {group.id}, COUNT: {group.Count()}");
                    return;
                }
                string str = "";
                foreach (int id in group)
                    str += $"{id}, ";
                Debug.LogError($"ADD/REM MISMATCH // GROUP ID: {group.id}, COUNT: {group.Count()}, PAIR NOT FOUND FOR NEXT IDS:\n{str}");
            }
        }
        static TableEventGroup Find(string groupId)
        {
            foreach (TableEventGroup group in _groups)
            {
                if (group.id == groupId)
                    return group;
            }
            return null;
        }

        public static TableEventGroup GetGroup(string groupId)
        {
            TableEventGroup group = Find(groupId);
            if (group == null)
            {
                group = new TableEventGroup(groupId);
                _groups.Add(group);
            }
            return group;
        }

        public static void Add(string groupId, int eventId)
        {
            if (Thread.CurrentThread.ManagedThreadId != PlayerLoopHelper.MainThreadId)
                return;

            TableEventGroup group = GetGroup(groupId);
            group.Add(eventId);
            if (debug)
                Debug.Log($"ADD // GROUP ID: {groupId}, EVENT ID: {eventId}, COUNT: {group.Count()}");
        }
        public static void Remove(string groupId, int eventId)
        {
            if (Thread.CurrentThread.ManagedThreadId != PlayerLoopHelper.MainThreadId)
                return;

            TableEventGroup group = Find(groupId);
            if (group == null) return;
            if (!debug)
                group.Remove(eventId);
            else if (!group.Remove(eventId))
                 Debug.LogError($"REM FAILED // GROUP ID: {groupId}, EVENT ID: {eventId}, COUNT: {group.Count()}");
            else Debug.Log($"REM // GROUP ID: {groupId}, EVENT ID: {eventId}, COUNT: {group.Count()}");
        }

        public static int Count(string groupId)
        {
            TableEventGroup group = Find(groupId);
            if (group == null) return 0;
            return group.Count();
        }
        public static int CountAll()
        {
            int count = 0;
            foreach (TableEventGroup group in _groups)
                count += group.Count();
            return count;
        }

        public static UniTask Await(string groupId, int eventsThreshold = 0)
        {
            TableEventGroup group = Find(groupId);
            if (group == null) return UniTask.CompletedTask;
            return group.Await(eventsThreshold);
        }
        public static UniTask AwaitAll()
        {
            UniTask[] tasks = new UniTask[_groups.Count];
            for (int i = 0; i < tasks.Length; i++)
                tasks[i] = _groups[i].Await();
            return UniTask.WhenAll(tasks);
        }
    }
}
