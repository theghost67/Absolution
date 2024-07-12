using Cysharp.Threading.Tasks;
using DG.Tweening;
using GreenOne;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий коллекцию элементов списка навыков (см. <see cref="TableTraitListElement"/>) у отрисовщика.<br/>
    /// Существует для удобства пользователя (добавляет изменение стаков навыков в очередь).
    /// </summary>
    public class TableTraitListSetDrawerElementsCollection : IEnumerable<TableTraitListElementDrawer>
    {
        public event EventHandler OnStarted;
        public event EventHandler OnEnded;
        public event EventHandler OnceComplete;

        public bool ContainsTraits => _storage.Count != 0;
        public bool IsAnySelected => IsAnyElementSelected();
        public bool IsAnyActivated => IsAnyElementActivated();

        readonly TableTraitListSetDrawer _drawer;
        readonly List<ITableTraitListElement> _storage;
        readonly Queue<QueueQuery> _queue;

        bool _isRunning;
        float _currentY;

        class QueueQuery : IEquatable<QueueQuery>
        {
            public readonly ITableTraitListElement element;
            public QueueOperation operation;
            public int stacks;

            public QueueQuery(ITableTraitListElement element, QueueOperation operation)
            {
                this.element = element;
                this.operation = operation;
            }
            public bool Equals(QueueQuery other)
            {
                return element.Equals(other.element);
            }
        }
        enum QueueOperation
        {
            Add,
            Adjust,
            Remove,
        }

        public TableTraitListSetDrawerElementsCollection(TableTraitListSetDrawer drawer)
        {
            _drawer = drawer;
            _currentY = 0.25f; // start pos
            _storage = new List<ITableTraitListElement>();
            _queue = new Queue<QueueQuery>();
        }
        public TableTraitListElementDrawer this[string id] => _storage.FirstOrDefault(e => e.Trait.Data.id == id)?.Drawer;

        public void EnqueueInstantly(ITableTraitListElement element)
        {
            if (element.Drawer == null)
                throw new NullReferenceException();

            bool isAdded = _storage.Contains(element);
            if (isAdded) return;

            TableTraitListElementDrawer drawer = element.Drawer;
            float posYDelta = drawer.GetSizeDelta().y;
            drawer.transform.localPosition = Vector3.up * _currentY;
            _storage.Add(element);
            _currentY -= posYDelta;
        }
        public void Enqueue(ITableTraitListElement element)
        {
            if (element.Drawer == null)
                throw new NullReferenceException();

            bool isAdded = _storage.Contains(element);
            if (!isAdded)
                 EnqueueElementForAdd(element);
            else EnqueueElementForAdjust(element);
        }
        public void Enqueue(ITableTraitListElement element, int stacksDelta)
        {
            if (element.Drawer == null)
                throw new NullReferenceException();

            // redraws this drawer by enqueueing/dequeueing ITableTraitListElement's and playing their animations
            bool isAdded = element.WasAdded(stacksDelta);
            bool isRemoved = !isAdded && element.WasRemoved(stacksDelta);

            if (isAdded)
                EnqueueElementForAdd(element);
            else if (isRemoved)
                EnqueueElementForRemove(element);
            else EnqueueElementForAdjust(element);
        }

        void EnqueueElementForAdd(ITableTraitListElement element)
        {
            TableTraitListElementDrawer elementDrawer = element.Drawer;
            QueueQuery query = new(element, QueueOperation.Add);

            elementDrawer.SetAlpha(0);
            elementDrawer.SetSortingOrder(_drawer.GetSortingOrder());
            EnqueueInternal(query);
        }
        void EnqueueElementForAdjust(ITableTraitListElement element)
        {
            QueueQuery query = new(element, QueueOperation.Adjust);
            query.stacks = element.Stacks.ClampedMin(0);
            EnqueueInternal(query);
        }
        void EnqueueElementForRemove(ITableTraitListElement element)
        {
            QueueQuery query = _queue.FirstOrDefault(q => q.element == element);
            bool isInQueue = query != null;
            if (isInQueue)
            {
                query.operation = QueueOperation.Remove;
                return;
            }

            query = new QueueQuery(element, QueueOperation.Remove);
            EnqueueInternal(query);
        }

        bool IsAnyElementSelected()
        {
            foreach (ITableTraitListElement element in _storage)
            {
                if (element.Drawer?.IsSelected ?? false)
                    return true;
            }
            return false;
        }
        bool IsAnyElementActivated()
        {
            foreach (ITableTraitListElement element in _storage)
            {
                if (element.Drawer?.IsActivated ?? false)
                    return true;
            }
            return false;
        }

        void EnqueueInternal(QueueQuery elementQuery)
        {
            _queue.Enqueue(elementQuery);
            if (!_isRunning)
                QueueLoop();
        }
        async UniTask QueueLoop()
        {
            _isRunning = true;
            OnStarted?.Invoke(this, EventArgs.Empty);

            await _drawer.ShowStoredElements().AsyncWaitForCompletion();
            while (!_drawer.IsDestroyed && _queue.Count > 0)
            {
                QueueQuery query = _queue.Dequeue();
                ITableTraitListElement element = query.element;
                TableTraitListElementDrawer elementDrawer = element.Drawer;

                if (query.operation == QueueOperation.Adjust)
                {
                    if (elementDrawer == null) continue;
                    Tween tween = elementDrawer.AnimAdjust(query.stacks);
                    if (tween != null) await tween.AsyncWaitForCompletion();
                    continue;
                }

                bool addToStorage = query.operation == QueueOperation.Add;
                float posYDelta = elementDrawer.GetSizeDelta().y;
                float elementUpperPoint = _currentY;

                if (addToStorage)
                {
                    elementDrawer.transform.localPosition = Vector3.up * _currentY;
                    _storage.Add(element);
                    _currentY -= posYDelta;
                    await elementDrawer.AnimAppear().AsyncWaitForCompletion();
                }
                else
                {
                    int indexInQueue = _storage.IndexOf(element);
                    for (int i = indexInQueue + 1; i < _storage.Count; i++)
                    {
                        TableTraitListElementDrawer drawer = _storage[i].Drawer;
                        drawer.AnimScroll(drawer.transform.localPosition.y + posYDelta);
                    }

                    _storage.Remove(element);
                    _currentY += posYDelta;
                    await elementDrawer.AnimDisappear().AsyncWaitForCompletion();
                    element.DestroyDrawer(false);
                }

                float elementLowerPoint = _currentY;
                // TODO: implement scrolling (elementUpperPoint && elementLowerPoint must be visible)
                // use _drawer.ScrollStoredElements

                //if (_animPosY < -0.36)
                ////inverts lowest pos, subtracts half of viewport size & text offsets
                //float scrollPosY = -_animPosY - 0.69f / 2 - 0.015f;
                //_animScrollTween = transform.DOMoveY(scrollPosY, 0.25f).SetEase(Ease.OutQuad);
            }

            if (OnceComplete == null)
                await UniTask.Delay(1000);

            if ((!_drawer.Owner?.IsSelected) ?? false)
                _drawer.HideStoredElements();

            _isRunning = false;
            OnEnded?.Invoke(this, EventArgs.Empty);

            OnceComplete?.Invoke(this, EventArgs.Empty);
            OnceComplete = null;
        }

        public IEnumerator<TableTraitListElementDrawer> GetEnumerator()
        {
            foreach (ITableTraitListElement element in _storage)
                yield return element.Drawer;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
