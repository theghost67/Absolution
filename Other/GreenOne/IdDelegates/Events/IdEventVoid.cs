using System;
using System.Collections.Generic;

namespace GreenOne
{
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventVoidHandler"/> (без возврата значения) и приоритетами.
    /// </summary>
    public class IdEventVoid : IdDelegate<IdEventVoidHandler>, IIdEventVoid
    {
        public IdEventVoid() : base() { }
        protected IdEventVoid(IdEventVoid other) : base(other) { }

        public void Invoke(object sender, EventArgs e)
        {
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isIncluded) continue;
                if (sub.isSubscribed)
                    sub.@delegate(sender, e);
                else unsubbedIds.Add(sub.id);
            }

            PostInvokeCleanUp(unsubbedIds);
        }
        public void InvokeIncluding(object sender, EventArgs e, string[] ids)
        {
            IncludeSubs(ids);
            Invoke(sender, e);
            RestoreSubs();
        }
        public void InvokeExcluding(object sender, EventArgs e, string[] ids)
        {
            ExcludeSubs(ids);
            Invoke(sender, e);
            RestoreSubs();
        }

        public override object Clone()
        {
            return new IdEventVoid(this);
        }
    }
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventVoidHandler"/> (без возврата значения и с параметром <typeparamref name="T"/>) и приоритетами.
    /// </summary>
    public class IdEventVoid<T> : IdDelegate<IdEventVoidHandler<T>>, IIdEventVoid<T>
    {
        public IdEventVoid() : base() { }
        protected IdEventVoid(IdEventVoid<T> other) : base(other) { }

        public void Invoke(object sender, T e)
        {
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isIncluded) continue;
                if (sub.isSubscribed)
                    sub.@delegate(sender, e);
                else unsubbedIds.Add(sub.id);
            }

            PostInvokeCleanUp(unsubbedIds);
        }
        public void InvokeIncluding(object sender, T e, string[] ids)
        {
            IncludeSubs(ids);
            Invoke(sender, e);
            RestoreSubs();
        }
        public void InvokeExcluding(object sender, T e, string[] ids)
        {
            ExcludeSubs(ids);
            Invoke(sender, e);
            RestoreSubs();
        }

        public override object Clone()
        {
            return new IdEventVoid<T>(this);
        }
    }

    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventVoidHandler"/> (без возврата значения) и приоритетами.
    /// </summary>
    public interface IIdEventVoid : IIdDelegate<IdEventVoidHandler> { }
    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventVoidHandler{T}"/> (без возврата значения и с параметром <typeparamref name="T"/>) и приоритетами.
    /// </summary>
    public interface IIdEventVoid<T> : IIdDelegate<IdEventVoidHandler<T>> { }
}
