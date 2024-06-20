using System;

namespace GreenOne.Console
{
    /// <summary>
    /// Структура, представляющая аргумент команды, переданный ей во время ввода.
    /// </summary>
    public readonly struct CommandArgInput : IEquatable<CommandArgInput>
    {
        public readonly CommandArg argRef;
        public readonly bool isValid;
        public readonly string input;
        public readonly object? value;

        public CommandArgInput(CommandArg argRef, string input)
        {
            this.argRef = argRef;
            this.input = input;
            isValid = argRef.TryParseValue(input, out value);
        }
        public bool Equals(CommandArgInput other)
        {
            if (argRef == null)
                return other.argRef == null;
            else return argRef.Equals(other.argRef);
        }
        public T ValueAs<T>() => (T)value;

        public override int GetHashCode()
        {
            return argRef.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            return obj is CommandArgInput input && Equals(input);
        }
        public override string ToString()
        {
            return isValid ? value!.ToString()! : "Invalid value.";
        }

        public static bool operator ==(CommandArgInput left, CommandArgInput right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CommandArgInput left, CommandArgInput right)
        {
            return !(left == right);
        }
    }
}