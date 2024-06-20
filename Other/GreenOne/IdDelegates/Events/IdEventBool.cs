using System;
using System.Collections.Generic;

namespace GreenOne
{
    // NOTE 1: if delegate has no subscribers, it will return true
    // NOTE 2: AND: once reached 'false', breaks loop    OR: once reached 'true', breaks loop

    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventFuncHandler{bool}"/> (с возвратом <see cref="bool"/>) и приоритетами.
    /// </summary>
    public class IdEventBool : IdDelegate<IdEventFuncHandler<bool>>, IIdEventBool
    {
        public IdEventBool() : base() { }
        protected IdEventBool(IdEventBool other) : base(other) { }

        public bool InvokeAND(object sender, EventArgs e)
        {
            if (Count == 0) return true;
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (!sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return false;
                }
            }
            PostInvokeCleanUp(unsubbedIds);
            return true;
        }
        public bool InvokeANDIncluding(object sender, EventArgs e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }
        public bool InvokeANDExcluding(object sender, EventArgs e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }

        public bool InvokeOR(object sender, EventArgs e)
        {
            if (Count == 0) return true;
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return true;
                }
            }
            PostInvokeCleanUp(unsubbedIds);
            return false;
        }
        public bool InvokeORIncluding(object sender, EventArgs e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }
        public bool InvokeORExcluding(object sender, EventArgs e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }

        public override object Clone()
        {
            return new IdEventBool(this);
        }
    }
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventFuncHandler{bool, T}"/> (с возвратом <see cref="bool"/> и параметром <typeparamref name="T"/>) и приоритетами.
    /// </summary>
    public class IdEventBool<T> : IdDelegate<IdEventFuncHandler<bool, T>>, IIdEventBool<T>
    {
        public IdEventBool() : base() { }
        protected IdEventBool(IdEventBool<T> other) : base(other) { }

        public bool InvokeAND(object sender, T e)
        {
            if (Count == 0) return true;
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (!sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return false;
                }
            }
            PostInvokeCleanUp(unsubbedIds);
            return true;
        }
        public bool InvokeANDIncluding(object sender, T e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }
        public bool InvokeANDExcluding(object sender, T e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }

        public bool InvokeOR(object sender, T e)
        {
            if (Count == 0) return true;
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return true;
                }
            }
            PostInvokeCleanUp(unsubbedIds);
            return false;
        }
        public bool InvokeORIncluding(object sender, T e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }
        public bool InvokeORExcluding(object sender, T e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }

        public override object Clone()
        {
            return new IdEventBool<T>(this);
        }
    }

    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventFuncHandler{bool}"/> (с возвратом <see cref="bool"/>) и приоритетами.
    /// </summary>
    public interface IIdEventBool : IIdDelegate<IdEventFuncHandler<bool>> { }
    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventFuncHandler{bool, T}"/> (с возвратом <see cref="bool"/> и параметром <typeparamref name="T"/>) и приоритетами.
    /// </summary>
    public interface IIdEventBool<T> : IIdDelegate<IdEventFuncHandler<bool, T>> { }
}
