using System;

namespace GreenOne.Console
{
    /// <summary>
    /// Исключение, возникающее при неправильном указании количества аргументов.
    /// </summary>
    public class ArgCountException : Exception
    {
        public readonly bool notEnough;

        public ArgCountException(bool notEnough) { this.notEnough = notEnough; }
        public ArgCountException(bool notEnough, string message) : base(message) { this.notEnough = notEnough; }
        public ArgCountException(bool notEnough, string message, Exception inner) : base(message, inner) { this.notEnough = notEnough; }
    }
}