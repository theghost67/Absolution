﻿using Cysharp.Threading.Tasks;
using GreenOne;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventVoidHandlerAsync"/> (без возврата значения) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.<br/>
    /// Вызов события добавляет его в <see cref="TableEventManager"/> до момента завершения вызова.
    /// </summary>
    public class TableEventVoid : IdDelegate<IdEventVoidHandlerAsync>, ITableEventVoid
    {
        public TableEventVoid() : base() { }
        protected TableEventVoid(TableEventVoid other) : base(other) { }

        public async UniTask Invoke(object sender, EventArgs e)
        {
            if (Count == 0) return;
            TableEventManager.Add(Id);
            List<string> unsubbedIds = new(Count);
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    Subscriber sub = GetSub(i);
                    if (!sub.isIncluded) continue;
                    if (!sub.isSubscribed)
                        unsubbedIds.Add(sub.id);
                    else await sub.@delegate(sender, e);
                }
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                PostInvokeCleanUp(unsubbedIds);
                TableEventManager.Remove(Id);
            }
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
            return new TableEventVoid(this);
        }
    }
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventVoidHandlerAsync"/> (без возврата значения и с параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.<br/>
    /// Вызов события добавляет его в <see cref="TableEventManager"/> до момента завершения вызова.
    /// </summary>
    public class TableEventVoid<T> : IdDelegate<IdEventVoidHandlerAsync<T>>, ITableEventVoid<T>
    {
        public TableEventVoid() : base() { }
        protected TableEventVoid(TableEventVoid<T> other) : base(other) { }

        public async UniTask Invoke(object sender, T e)
        {
            if (Count == 0) return;
            TableEventManager.Add(Id);
            List<string> unsubbedIds = new(Count);
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    Subscriber sub = GetSub(i);
                    if (!sub.isIncluded) continue;
                    if (!sub.isSubscribed)
                        unsubbedIds.Add(sub.id);
                    else await sub.@delegate(sender, e);
                }
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                PostInvokeCleanUp(unsubbedIds);
                TableEventManager.Remove(Id);
            }
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
            return new TableEventVoid<T>(this);
        }
    }

    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventVoidHandler"/> (без возврата значения) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public interface ITableEventVoid : IIdEventVoidAsync { }
    /// <summary>
    /// Реализует объект как событие без возможности вызова с уникальными делегатами<br/>
    /// типа <see cref="IdEventVoidHandler{T}"/> (без возврата значения и с параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.
    /// </summary>
    public interface ITableEventVoid<T> : IIdEventVoidAsync<T> { }
}
