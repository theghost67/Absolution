using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        Tween _finishTween;

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

            _finishTween.Kill();
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
            _finishTween = DOVirtual.DelayedCall(0.25f, FinishTweenOnComplete);
            _isRunning = false;
            _isAnyRunning = false;
        }
        void FinishTweenOnComplete()
        {
            if (drawer == null) return;
            if (drawer.IsSelected)
                drawer.IsSelected = true;
            else if (!drawer.HasInitiationPreview() && !(drawer.Traits?.queue.IsRunning ?? false)) 
                drawer.HideBg();
        }
    }
}
