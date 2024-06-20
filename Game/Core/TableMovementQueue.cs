using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Game
{
    // USE ONLY WHEN: card/trait/trait list event fires (onStacksChange, onTargetStateChange, onUse)
    // in other cases use cards/traits async events

    ///// <summary>
    ///// Класс, представляющий глобальную очередь асинхронных действий. Очередь позволяет задержать анимации (включая игровые последствия) для того,<br/>
    ///// чтобы игрок лучше понимал о происходящем. Без использования очереди анимации и эффекты могут применяться одновременно, путая игрока.<br/>
    ///// </summary>
    //public static class TableMovementQueue
    //{
    //    public static event Action OnStarted;
    //    public static event Action OnEnded;
    //    public static event Action OnceEnqueued;
    //    public static event Action OnceComplete;

    //    public static bool IsRunning => _isRunning;
    //    public static bool IsEmpty => _queue.Count == 0;
    //    public static int Count => _queue.Count;

    //    public static int PreviousId => _previousId;
    //    public static int CurrentId => _currentId;
    //    public static int NextId => _nextId;

    //    static readonly Queue<Element> _queue = new();
    //    static int _previousId;
    //    static int _currentId;
    //    static int _nextId;
    //    static bool _isRunning;

    //    readonly struct Element
    //    {
    //        public readonly Func<UniTask> factory;
    //        public readonly int id;
    //        public Element(Func<UniTask> factory, int id)
    //        {
    //            this.factory = factory;
    //            this.id = id;
    //        }
    //    }

    //    public static void Enqueue(Action action, int id = -1)
    //    {
    //        return Enqueue(() => { action(); return UniTask.CompletedTask; }, id);
    //    }
    //    public static void Enqueue(Func<UniTask> factory, int id = -1)
    //    {
    //        Element element = new(factory, id);
    //        _queue.Enqueue(element);
    //        OnceEnqueued?.Invoke();
    //        OnceEnqueued = null;

    //        if (!_isRunning) 
    //            QueueLoop();
    //        return element;
    //    }

    //    public async static UniTask Await()
    //    {
    //        while (_isRunning)
    //            await UniTask.Yield();
    //    }
    //    private static async UniTask QueueLoop()
    //    {
    //        _isRunning = true;
    //        OnStarted?.Invoke();

    //        _currentId = -1;
    //        while (_queue.Count > 0)
    //        {
    //            Element element = _queue.Dequeue();
    //            _previousId = _currentId;
    //            _currentId = element.id;
    //            _nextId = _queue.Count != 0 ? _queue.Peek().id : -1;
    //            await element.factory();
    //            element.isCompleted = true;
    //        }

    //        _isRunning = false;
    //        OnEnded?.Invoke();

    //        OnceComplete?.Invoke();
    //        OnceComplete = null;
    //    }
    //}
}
