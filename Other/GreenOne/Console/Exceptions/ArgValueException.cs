using System;

namespace GreenOne.Console
{
    /// <summary>
    /// Исключение, возникающее при недопустимом значении аргумента.
    /// </summary>
    public class ArgValueException : Exception
    {
        public readonly string argId;

        public ArgValueException(string argId) { this.argId = argId; }
        public ArgValueException(string argId, string message) : base(message) { this.argId = argId; }
        public ArgValueException(string argId, string message, Exception inner) : base(message, inner) { this.argId = argId; }
    }
}