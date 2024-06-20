using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace GreenOne
{
    // NOTE 1: if delegate has no subscribers, it will return true
    // NOTE 2: AND: once reached 'false', breaks loop    OR: once reached 'true', breaks loop

    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventFuncHandlerAsync{bool}"/> (с возвратом <see cref="bool"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асихнронными, позволяя выполнять их последовательно.
    /// </summary>
    public class IdEventBoolAsync : IdDelegate<IdEventFuncHandlerAsync<bool>>, IIdEventBoolAsync
    {
        public IdEventBoolAsync() : base() { }
        protected IdEventBoolAsync(IdEventBoolAsync other) : base(other) { }

        public async UniTask<bool> InvokeAND(object sender, EventArgs e)
        {
            if (Count == 0) return true;
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (!await sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return false;
                }
            }
            PostInvokeCleanUp(unsubbedIds);
            return false;
        }
        public async UniTask<bool> InvokeANDIncluding(object sender, EventArgs e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = await InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }
        public async UniTask<bool> InvokeANDExcluding(object sender, EventArgs e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = await InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }

        public async UniTask<bool> InvokeOR(object sender, EventArgs e)
        {
            bool result = true;
            List<string> unsubbedIds = new(Count);

            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (await sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return true;
                }
            }

            PostInvokeCleanUp(unsubbedIds);
            return result;
        }
        public async UniTask<bool> InvokeORIncluding(object sender, EventArgs e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = await InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }
        public async UniTask<bool> InvokeORExcluding(object sender, EventArgs e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = await InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }

        public override object Clone()
        {
            return new IdEventBoolAsync(this);
        }
    }
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventFuncHandlerAsync{bool, T}"/> (с возвратом <see cref="bool"/> и параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты добавляются в очередь <see cref="TableQueue"/>, позволяя выполнять их последовательно.
    /// </summary>
    public class IdEventBoolAsync<T> : IdDelegate<IdEventFuncHandlerAsync<bool, T>>, IIdEventBoolAsync<T>
    {
        public IdEventBoolAsync() : base() { }
        protected IdEventBoolAsync(IdEventBoolAsync<T> other) : base(other) { }

        public async UniTask<bool> InvokeAND(object sender, T e)
        {
            bool result = true;
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (!await sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return false;
                }
            }

            PostInvokeCleanUp(unsubbedIds);
            return result;
        }
        public async UniTask<bool> InvokeANDIncluding(object sender, T e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = await InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }
        public async UniTask<bool> InvokeANDExcluding(object sender, T e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = await InvokeAND(sender, e);
            RestoreSubs();
            return result;
        }

        public async UniTask<bool> InvokeOR(object sender, T e)
        {
            bool result = true;
            List<string> unsubbedIds = new(Count);

            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (await sub.@delegate(sender, e))
                {
                    PostInvokeCleanUp(unsubbedIds);
                    return true;
                }
            }

            PostInvokeCleanUp(unsubbedIds);
            return result;
        }
        public async UniTask<bool> InvokeORIncluding(object sender, T e, params string[] ids)
        {
            IncludeSubs(ids);
            bool result = await InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }
        public async UniTask<bool> InvokeORExcluding(object sender, T e, params string[] ids)
        {
            ExcludeSubs(ids);
            bool result = await InvokeOR(sender, e);
            RestoreSubs();
            return result;
        }

        public override object Clone()
        {
            return new IdEventBoolAsync<T>(this);
        }
    }

    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventFuncHandler{bool}"/> (с возвратом <see cref="bool"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public interface IIdEventBoolAsync : IIdDelegate<IdEventFuncHandlerAsync<bool>> { }
    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventFuncHandler{bool, T}"/> (с возвратом <see cref="bool"/> и параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public interface IIdEventBoolAsync<T> : IIdDelegate<IdEventFuncHandlerAsync<bool, T>> { }
}
