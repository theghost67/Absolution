using System.Collections.Generic;

namespace GreenOne.Console
{
    /// <summary>
    /// Статический класс, представляющий список всевозможных команд (см. <see cref="Command"/>).
    /// </summary>
    public static class CommandList
    {
        public static IReadOnlyDictionary<string, Command> Set => _set;
        static readonly Dictionary<string, Command> _set = new();

        public static void Add(Command cmd)
        {
            _set.Add(cmd.id, cmd);
        }
        public static void Remove(string id)
        {
            _set.Remove(id);
        }
        public static void Clear()
        {
            _set.Clear();
        }
    }
}
