using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GreenOne
{
    /// <summary>
    /// Абстрактный базовый класс для событий с уникальными делегатами типа <typeparamref name="D"/> и приоритетами.
    /// </summary>
    public abstract class IdDelegate<D> : IIdDelegate<D>, IEquatable<IdDelegate<D>> where D : Delegate
    {
        public const int TOP_PRIORITY = 256;
        public int Count => _subs.Count;
        protected int Id => _id;

        static int _idCounter;
        readonly List<Subscriber> _subs;
        readonly int _id;

        protected class Subscriber : IIdSubscriber<D>, IComparable<Subscriber>
        {
            public readonly string id;
            public readonly D @delegate;
            public readonly int priority;
            public bool isSubscribed;
            public bool isIncluded;

            string IIdSubscriber<D>.Id => id;
            D IIdSubscriber<D>.Delegate => @delegate;
            int IIdSubscriber<D>.Priority => priority;
            bool IIdSubscriber<D>.IsSubscribed => isSubscribed;

            public Subscriber(string id, D @delegate, int priority)
            {
                this.id = id;
                this.@delegate = @delegate;
                this.priority = priority;
                isSubscribed = true;
                isIncluded = true;
            }
            public override string ToString()
            {
                return $"id: {id}, priority: {priority} ({GetType()})";
            }
            public object Clone() => new Subscriber(id, @delegate, priority) { isSubscribed = isSubscribed, isIncluded = isIncluded };
            public int CompareTo(Subscriber other)
            {
                if (priority > other.priority)
                     return 1;
                else return -1;
            }
        }

        public IdDelegate()
        {
            _subs = new List<Subscriber>();
            _id = _idCounter++;
        }
        protected IdDelegate(IdDelegate<D> other)
        {
            _subs = new List<Subscriber>(other._subs.Capacity);
            foreach (Subscriber sub in other._subs)
                _subs.Add((Subscriber)sub.Clone());
            _id = other._id;
        }

        public bool Add(string id, D @delegate, int priority = 0)
        {
            if (id == null || id[0] != '_')
                 return AddBase(id, @delegate, priority);
            else throw new NotSupportedException("Action id must not start with \'_\' character.");
        }
        protected bool Add(IIdSubscriber<D> sub)
        {
            return AddBase(sub.Id, sub.Delegate, sub.Priority);
        }
        bool AddBase(string id, D @delegate, int priority = 0)
        {
            if (@delegate == null) // Impossible to add delegate with null reference.
                return false;
            if (priority > TOP_PRIORITY)
                return false; // Delegate priority should not be greater than {TOP_PRIORITY}.

            id ??= "_" + _idCounter++.ToString();
            foreach (Subscriber sub in _subs)
            {
                if (sub.id == id)
                    return false;
            }
            _subs.InsertionSort(new Subscriber(id, @delegate, priority));
            return true;
        }

        public abstract object Clone();
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return $"Id: {Id}, Count: {Count} ({GetType()})";
        }

        public void Dispose()
        {
            _subs.Clear();
        }
        public bool Equals(IdDelegate<D> other)
        {
            return _id == other._id; // also compare all subs id's?
        }

        public bool Remove(string id)
        {
            Subscriber subscriber = null;
            foreach (Subscriber sub in _subs)
            {
                if (sub.id == id)
                {
                    subscriber = sub;
                    break;
                }
            }

            bool unsubscribe = subscriber != null && subscriber.isSubscribed;
            if (unsubscribe) subscriber.isSubscribed = false;
            return unsubscribe;
        }
        public void Clear()
        {
            _subs.Clear();
        }

        protected void IncludeSubs(string[] ids)
        {
            foreach (Subscriber sub in _subs)
                sub.isIncluded = ids.Contains(sub.id);
        }
        protected void ExcludeSubs(string[] ids)
        {
            foreach (Subscriber sub in _subs)
                sub.isIncluded = ids.Contains(sub.id);
        }
        protected void RestoreSubs()
        {
            foreach (Subscriber sub in _subs)
                sub.isIncluded = true;
        }

        protected void PostInvokeCleanUp(List<string> unsubbedIds)
        {
            int unsubbedCount = unsubbedIds.Count;
            for (int i = 0; i < unsubbedCount; i++)
            {
                for (int j = 0; j < Count; j++)
                {
                    if (_subs[j].id == unsubbedIds[i])
                    {
                        _subs.RemoveAt(j);
                        break;
                    }
                }
            }
        }
        protected Subscriber GetSub(int index) => _subs[index];

        public IEnumerator<IIdSubscriber<D>> GetEnumerator()
        {
            return _subs.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами типа <typeparamref name="D"/> и приоритетами.
    /// </summary>
    public interface IIdDelegate<D> : IEnumerable<IIdSubscriber<D>>, ICloneable, IDisposable where D : Delegate
    {
        public int Count { get; }
        public bool Add(string id, D @delegate, int priority = 0);
        public bool Remove(string id);
        public void Clear();
    }
}
