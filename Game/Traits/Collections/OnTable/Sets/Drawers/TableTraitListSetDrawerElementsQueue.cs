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
    /// Класс, представляющий коллекцию элементов списка навыков (см. <see cref="TableTraitListElement"/>) с очередью.<br/>
    /// Существует для удобства пользователя (добавляет изменение зарядов навыков в очередь).
    /// </summary>
    public class TableTraitListSetDrawerElementsQueue : IEnumerable<TableTraitListElementDrawer>
    {
        public event EventHandler OnStarted;
        public event EventHandler OnEnded;
        public event EventHandler OnceComplete;

        public bool IsEmpty => _storage.Count == 0;
        public bool IsRunning => _isRunning;

        readonly TableTraitListSetDrawer _drawer;
        readonly List<ITableTraitListElement> _storage;
        readonly Queue<QueueQuery> _queue;
        bool _isRunning;

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

        public TableTraitListSetDrawerElementsQueue(TableTraitListSetDrawer drawer)
        {
            _drawer = drawer;
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
            _drawer.AddToViewport(drawer);
            _storage.Add(element);
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

            elementDrawer.Alpha = 0;
            elementDrawer.SortingOrder = _drawer.SortingOrder;
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
                    await _drawer.ScrollTo(elementDrawer).AsyncWaitForCompletion();
                    await elementDrawer.AnimAdjust(query.stacks).AsyncWaitForCompletion();
                    continue;
                }

                bool addToStorage = query.operation == QueueOperation.Add;
                if (addToStorage)
                {
                    _storage.Add(element);
                    _drawer.AddToViewport(elementDrawer);
                    await _drawer.ScrollTo(elementDrawer).AsyncWaitForCompletion();
                    await elementDrawer.AnimAppear().AsyncWaitForCompletion();
                }
                else
                {
                    _storage.Remove(element);
                    await _drawer.ScrollTo(elementDrawer).AsyncWaitForCompletion();
                    await elementDrawer.AnimDisappear().AsyncWaitForCompletion();
                    await _drawer.RemoveFromViewportAsync(elementDrawer);
                    element.DestroyDrawer(false);
                }
            }

            if (OnceComplete == null)
                await UniTask.Delay(1000);

            if (!_drawer.IsDestroyed && _drawer.Owner != null && !_drawer.Owner.IsSelected)
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
