using System.Collections.Generic;

namespace GreenOne.Console
{
    /// <summary>
    /// Статический класс, представляющий список всевозможных команд (см. <see cref="Command"/>).
    /// </summary>
    public static class Commands
    {
        public static IEnumerable<Command> List => _list;
        static readonly List<Command> _list = new();

        public static void Add(Command cmd)
        {
            _list.Add(cmd);
        }
        public static void Remove(Command cmd)
        {
            _list.Remove(cmd);
        }
        public static void Clear()
        {
            _list.Clear();
        }
    }
}
