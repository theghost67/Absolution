using Cysharp.Threading.Tasks;
using Game.Traits;
using GreenOne;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий очередь анимаций навыков (см. <see cref="ITableTrait"/>) для отрисовщика карты поля (см. <see cref="TableFieldCard"/>).<br/>
    /// Проигрывает анимации типа <see cref="TableFieldCardDrawerQueueElement"/> по очереди на карте-владельце.
    /// </summary>
    public class TableFieldCardDrawerQueue
    {
        public readonly TableFieldCardDrawer drawer;
        public bool IsRunning => _isRunning;
        public static bool IsAnyRunning => _isAnyRunning;

        static bool _isAnyRunning;
        bool _isRunning;
        readonly Queue<TableFieldCardDrawerQueueElement> _queue;

        public TableFieldCardDrawerQueue(TableFieldCardDrawer drawer)
        {
            this.drawer = drawer;
            _queue = new Queue<TableFieldCardDrawerQueueElement>();
        }
        public void Enqueue(TableFieldCardDrawerQueueElement element)
        {
            _queue.Enqueue(element);
            if (!_isRunning)
                QueueLoop();
        }
        public async UniTask Await()
        {
            while (_isRunning)
                await UniTask.Yield();
        }

        async void QueueLoop()
        {
            _isAnyRunning = true;
            _isRunning = true;
            TableTraitListSetDrawer setDrawer = drawer.Traits;

            if (setDrawer == null) goto End;
            setDrawer.HideStoredElementsInstantly();
            drawer.ShowBgInstantly();

            GameObject lastPrefab = null;
            while (_queue.Count > 0)
            {
                if (setDrawer == null) goto End;
                if (lastPrefab != null) lastPrefab.Destroy();
                TableFieldCardDrawerQueueElement element = _queue.Dequeue();
                lastPrefab = element.CreateAnimationPrefab();
                await element.PlayAnimation(lastPrefab);
            }

            End:
            if (!drawer.HasInitiationPreview())
                drawer.HideBg();

            _isRunning = false;
            _isAnyRunning = false;
        }
    }
}
