using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GreenOne
{
    // NOTE: to save floats/doubles, use Utils.ToDotString()
    /// <summary>
    /// Класс, представляющий словарь &lt;<see cref="string"/>, <see cref="object"/>&gt;, который можно (де)сериализовать и использовать как основу для создания объектов.<br/>Значение может иметь тип <see cref="SerializationDict"/>, что позволяет создавать пакетные данные.
    /// </summary>
    [Serializable] public sealed class SerializationDict : Dictionary<string, object>
    {
        public SerializationDict() : base() { }
        public SerializationDict(int capacity) : base(capacity) { }
        public SerializationDict(SerializationDict dict) : base(dict) { }

        public T DeserializeKeyAs<T>(string key)
        {
            if (TryGetValue(key, out object value))
                return JsonConvert.DeserializeObject<T>(value.ToString());
            else return default;
        }
        public SerializationDict DeserializeKeyAsDict(string key)
        {
            return DeserializeKeyAs<SerializationDict>(key);
        }
    }
}
