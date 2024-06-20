using Game.Territories;

namespace Game.Menus
{
    /// <summary>
    /// Интерфейс, реализующий меню как меню с территорией.
    /// </summary>
    public interface IMenuWithTerritory : IMenu
    {
        public TableTerritory Territory { get; }
    }
}