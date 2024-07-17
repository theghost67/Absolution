using System;
using System.Linq;
using System.Text;

namespace GreenOne.Console
{
    /// <summary>
    /// Абстрактный класс, представляющий аргумент команды терминала.
    /// </summary>
    public abstract class CommandArg : IEquatable<CommandArg>
    {
        public bool HasDefault => fixedValues.Length != 0;
        public FixedValue Default => HasDefault ? fixedValues[0] : default;

        public readonly bool isRequired;
        public readonly bool isFlag;
        public readonly bool isFixed;
        public readonly bool isDefault;

        public readonly Command command;
        public readonly ValueType type;
        public readonly string id;
        public readonly string desc;
        public readonly FixedValue[] fixedValues;

        /// <summary>
        /// Структура, представляющая одно из фиксированных значений аргумента.
        /// </summary>
        public readonly struct FixedValue : IEquatable<FixedValue>
        {
            public readonly string value;
            public readonly string desc;

            public FixedValue(string value, string desc)
            {
                this.value = value;
                this.desc = desc;
            }
            public bool Equals(FixedValue other)
            {
                return value == other.value;
            }

            public override bool Equals(object? obj)
            {
                return obj is FixedValue value && Equals(value);
            }
            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            public override string ToString()
            {
                return value;
            }
            public string ToFullString()
            {
                return $"{value} - {desc}";
            }

            public static bool operator ==(FixedValue left, FixedValue right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(FixedValue left, FixedValue right)
            {
                return !(left == right);
            }
        }
        /// <summary>
        /// Содержит флаги, определяющие тип значения аргумента.
        /// </summary>
        public enum ValueType
        {
            Optional = 0, // argument can be passed  
            Required = 1, // argument must be passed
            Flag = 2,     // argument cannot have a value
            Fixed = 4,    // argument can have only a value listed in _fixedValues
            Default = 8,  // (use with Optional flag) if argument wasn't passed at input, it will pass automatically with _fixedValues[0] value
        }

        public CommandArg(Command command, ValueType type, string id, string desc)
        {
            this.command = command;
            this.id = id;
            this.desc = desc;

            this.type = type;
            isRequired = type.HasFlag(ValueType.Required);
            isFlag = type.HasFlag(ValueType.Flag);
            isFixed = type.HasFlag(ValueType.Fixed);
            isDefault = type.HasFlag(ValueType.Default);

            if (isFlag && (type ^ ValueType.Flag) != 0)
                throw new InvalidOperationException($"Arguments with flag value cannot have any other flags in {nameof(ValueType)}.");

            if (isDefault && isRequired)
                throw new InvalidOperationException($"Arguments with default value cannot have {nameof(ValueType.Required)} flag");

            fixedValues = FixedValuesCreator();
        }
        public virtual bool TryParseValue(string str, out object? value)
        {
            bool isNotFixedValue = isFixed && !fixedValues.Select(fv => fv.value).Contains(str);
            bool isNotFlagValue = isFlag && str != string.Empty;
            if (isNotFixedValue || isNotFlagValue)
            {
                value = null;
                return false;
            }

            value = str;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new(id);
            if (isDefault) sb.Append('!');
            else if (!isRequired) sb.Append('?');

            if (isFixed)
            {
                sb.Insert(0, '{');
                sb.Append('}');
            }
            else if (isFlag)
            {
                sb.Insert(0, '<');
                sb.Append('>');
            }
            else
            {
                sb.Insert(0, '[');
                sb.Append(']');
            }

            return sb.ToString();
        }
        public string ToFullString()
        {
            string str = ToString();
            string smallSpace = new(' ', command.id.Length + 1); // empty space
            StringBuilder sb = new($"{smallSpace}{str}: {desc}");

            string bigSpace = new(' ', smallSpace.Length + str.Length + 2); // empty space + colon
            if (isFixed)
            {
                foreach (var value in fixedValues)
                    sb.Append($"\n{bigSpace}{value.ToFullString()}");
            }
            if (isDefault)
                sb.Append($"\n{bigSpace}Значение по умолчанию: {Default.value}");

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as CommandArg);
        }
        public bool Equals(CommandArg? other)
        {
            return id == other?.id;
        }

        protected virtual FixedValue[] FixedValuesCreator() => Array.Empty<FixedValue>();
    }
}