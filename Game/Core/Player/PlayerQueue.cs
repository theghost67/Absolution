using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий очередь действий игрока.<br/>
    /// Очередь действий ожидает завершение всех асинхронных событий стола перед удалением элемента из очереди (см. <see cref="TableEventManager"/>).<br/>
    /// Очередь принудительно очищается, если хотя бы один из элементов провалил своё выполнение.
    /// </summary>
    public static class PlayerQueue
    {
        static readonly Queue<PlayerAction> _queue = new();
        static bool _isRunning;

        public static void Enqueue(PlayerAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException(nameof(action));
            _queue.Enqueue(action);
            if (!_isRunning)
                _ = QueueLoop();
        }
        static async UniTask QueueLoop()
        {
            _isRunning = true;
            while (_queue.Count != 0)
            {
                await TableEventManager.AwaitAll();
                PlayerAction action = _queue.Dequeue();
                if (action.conditionFunc == null)
                    throw new System.NullReferenceException();
                if (action.conditionFunc())
                    action.successFunc?.Invoke();
                else
                {
                    action.failFunc?.Invoke();
                    while (_queue.Count != 0)
                        _queue.Dequeue().abortFunc?.Invoke();
                }
                if (action.msDelay > 0)
                    await UniTask.Delay(action.msDelay);
            }
            _isRunning = false;
        }
    }
}
