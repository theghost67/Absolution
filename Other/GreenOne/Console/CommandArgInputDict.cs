using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GreenOne.Console
{
    /// <summary>
    /// Запечатанный класс, представляющий словарь аргументов команды только для чтения, переданных ей во время ввода.
    /// </summary>
    public sealed class CommandArgInputDict : IReadOnlyDictionary<string, CommandArgInput>
    {
        readonly Dictionary<string, CommandArgInput> _args;
        public CommandArgInputDict(Dictionary<string, CommandArgInput> args)
        {
            _args = args;
        }

        public CommandArgInput this[string key] => _args[key];

        public IEnumerable<string> Keys => _args.Keys;
        public IEnumerable<CommandArgInput> Values => _args.Values;
        public int Count => _args.Count;

        public bool ContainsKey(string key)
        {
            return _args.ContainsKey(key);
        }
        public bool TryGetValue(string key, out CommandArgInput value)
        {
            return _args.TryGetValue(key, out value);
        }

        public bool ContainsInput<T>() where T : CommandArg
        {
            var value = _args.Values.FirstOrDefault(argI => argI.argRef is T);
            return value != default;
        }
        public bool TryGetInput<T>(out CommandArgInput value) where T : CommandArg
        {
            value = _args.Values.FirstOrDefault(argI => argI.argRef is T);
            return value != default;
        }
        public CommandArgInput GetInput<T>() where T : CommandArg
        {
            return _args.Values.First(argI => argI.argRef is T);
        }

        public IEnumerator<KeyValuePair<string, CommandArgInput>> GetEnumerator()
        {
            return _args.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_args).GetEnumerator();
        }
    }
}
