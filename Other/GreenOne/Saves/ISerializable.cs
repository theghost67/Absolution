using System;

namespace GreenOne
{
    /// <summary>
    /// Реализует сохранение объекта путём создания и использования <see cref="SerializationDict"/>.
    /// </summary>
    [Obsolete("Rewrite as JSON")] public interface ISerializable
    {
        public SerializationDict Serialize();
    }
}
