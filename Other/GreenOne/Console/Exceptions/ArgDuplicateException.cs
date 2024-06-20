using System;

namespace GreenOne.Console
{
    /// <summary>
    /// Исключение, возникающее при дублированном указании значения аргумента.
    /// </summary>
    public class ArgDuplicateException : Exception
    {
        public readonly string argId;

        public ArgDuplicateException(string argId) { this.argId = argId; }
        public ArgDuplicateException(string argId, string message) : base(message) { this.argId = argId; }
        public ArgDuplicateException(string argId, string message, Exception inner) : base(message, inner) { this.argId = argId; }
    }
}