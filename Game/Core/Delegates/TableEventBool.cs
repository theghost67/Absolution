using Cysharp.Threading.Tasks;
using GreenOne;
using System;
using System.Collections.Generic;

namespace Game
{
    // NOTE 1: if delegate has no subscribers, it will return true
    // NOTE 2: AND: once reached 'false', breaks loop    OR: once reached 'true', breaks loop

    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventFuncHandlerAsync{bool}"/> (с возвратом <see cref="bool"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.<br/>
    /// Вызов события добавляет его в <see cref="TableEventManager"/> до момента завершения вызова.
    /// </summary>
    public class TableEventBool : IdDelegate<IdEventFuncHandlerAsync<bool>>, IIdEventBoolAsync
    {
        public TableEventBool() : base() { }
        protected TableEventBool(TableEventBool other) : base(other) { }

        public async UniTask<bool> InvokeAND(object sender, EventArgs e)
        {
            if (Count == 0) return true;
            bool result = true;

            TableEventManager.Add();
            List<string> unsubbedIds = new(Count);

            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (!await sub.@delegate(sender, e))
                {
                    result = false;
                    goto End;
                }
            }

            End:
            PostInvokeCleanUp(unsubbedIds);
            TableEventManager.Remove();
            return result;
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
            if (Count == 0) return true;
            bool result = false;

            List<string> unsubbedIds = new(Count);
            TableEventManager.Add();

            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (await sub.@delegate(sender, e))
                {
                    result = true;
                    goto End;
                }
            }

            End:
            PostInvokeCleanUp(unsubbedIds);
            TableEventManager.Remove();

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
            return new TableEventBool(this);
        }
    }
    /// <summary>
    /// Событие с уникальными делегатами типа <see cref="IdEventFuncHandlerAsync{bool, T}"/> (с возвратом <see cref="bool"/> и параметром <typeparamref name="T"/>) и приоритетами.<br/>
    /// Вызываемые делегаты являются асинхронными, позволяя выполнять их последовательно.<br/>
    /// Вызов события добавляет его в <see cref="TableEventManager"/> до момента завершения вызова.
    /// </summary>
    public class TableEventBool<T> : IdDelegate<IdEventFuncHandlerAsync<bool, T>>, IIdEventBoolAsync<T>
    {
        public TableEventBool() : base() { }
        protected TableEventBool(TableEventBool<T> other) : base(other) { }

        public async UniTask<bool> InvokeAND(object sender, T e)
        {
            if (Count == 0) return true;
            bool result = true;

            TableEventManager.Add();
            List<string> unsubbedIds = new(Count);

            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (!await sub.@delegate(sender, e))
                {
                    result = false;
                    goto End;
                }
            }

            End:
            PostInvokeCleanUp(unsubbedIds);
            TableEventManager.Remove();

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
            if (Count == 0) return true;
            bool result = false;

            List<string> unsubbedIds = new(Count);
            TableEventManager.Add();

            for (int i = 0; i < Count; i++)
            {
                Subscriber sub = GetSub(i);
                if (!sub.isSubscribed)
                    unsubbedIds.Add(sub.id);
                else if (await sub.@delegate(sender, e))
                {
                    result = true;
                    goto End;
                }
            }

            End:
            PostInvokeCleanUp(unsubbedIds);
            TableEventManager.Remove();

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
            return new TableEventBool<T>(this);
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
