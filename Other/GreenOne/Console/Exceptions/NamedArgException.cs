using System;

namespace GreenOne.Console
{
    /// <summary>
    /// Исключение, возникающее при ошибке обработке аргумента как именнованного (с = как разделителем ID/значение).
    /// </summary>
    public class NamedArgException : Exception
    {
        public NamedArgException() { }
        public NamedArgException(string message) : base(message) { }
        public NamedArgException(string message, Exception inner) : base(message, inner) { }
    }
}