using System;

namespace GreenOne.Console
{
	/// <summary>
	/// Исключение, возникающее при ошибке обработке аргумента как комплексного (с " как ограничителем).
	/// </summary>
	public class ComplexArgException : Exception
	{
		public ComplexArgException() { }
		public ComplexArgException(string message) : base(message) { }
		public ComplexArgException(string message, Exception inner) : base(message, inner) { }
	}
}
