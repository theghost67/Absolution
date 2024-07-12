using System;

namespace GreenOne
{
    /// <summary>
    /// Реализует объект как подписчика события <see cref="IdDelegate{T}"/>.
    /// </summary>
    public interface IIdSubscriber<T> : ICloneable where T : Delegate
    {
        public string Id { get; }
        public T Delegate { get; }
        public int Priority { get; }
        public bool IsSubscribed { get; }
    }
}
