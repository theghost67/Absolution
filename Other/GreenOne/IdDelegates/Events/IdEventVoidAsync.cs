using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace GreenOne
{
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventVoidHandlerAsync"/> (без возврата значения) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public class IdEventVoidAsync : IdDelegate<IdEventVoidHandlerAsync>, IIdEventVoidAsync
    {
        public IdEventVoidAsync() : base() { }
        protected IdEventVoidAsync(IdEventVoidAsync other) : base(other) { }

        public async UniTask Invoke(object sender, EventArgs e)
        {
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isIncluded) continue;
                if (sub.isSubscribed)
                    await sub.@delegate(sender, e);
                else unsubbedIds.Add(sub.id);
            }
            PostInvokeCleanUp(unsubbedIds);
        }
        public async UniTask InvokeIncluding(object sender, EventArgs e, string[] ids)
        {
            IncludeSubs(ids);
            await Invoke(sender, e);
            RestoreSubs();
        }
        public async UniTask InvokeExcluding(object sender, EventArgs e, string[] ids)
        {
            ExcludeSubs(ids);
            await Invoke(sender, e);
            RestoreSubs();
        }

        public override object Clone()
        {
            return new IdEventVoidAsync(this);
        }
    }
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventVoidHandlerAsync"/> (без возврата значения и с параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public class IdEventVoidAsync<T> : IdDelegate<IdEventVoidHandlerAsync<T>>, IIdEventVoidAsync<T>
    {
        public IdEventVoidAsync() : base() { }
        protected IdEventVoidAsync(IdEventVoidAsync<T> other) : base(other) { }

        public async UniTask Invoke(object sender, T e)
        {
            List<string> unsubbedIds = new(Count);
            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isIncluded) continue;
                if (sub.isSubscribed)
                    await sub.@delegate(sender, e);
                else unsubbedIds.Add(sub.id);
            }
            PostInvokeCleanUp(unsubbedIds);
        }
        public async UniTask InvokeIncluding(object sender, T e, string[] ids)
        {
            IncludeSubs(ids);
            await Invoke(sender, e);
            RestoreSubs();
        }
        public async UniTask InvokeExcluding(object sender, T e, string[] ids)
        {
            ExcludeSubs(ids);
            await Invoke(sender, e);
            RestoreSubs();
        }

        public override object Clone()
        {
            return new IdEventVoidAsync<T>(this);
        }
    }

    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventVoidHandler"/> (без возврата значения) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public interface IIdEventVoidAsync : IIdDelegate<IdEventVoidHandlerAsync> { }
    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventVoidHandler{T}"/> (без возврата значения и с параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public interface IIdEventVoidAsync<T> : IIdDelegate<IdEventVoidHandlerAsync<T>> { }
}
