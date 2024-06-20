using Game.Menus;
using System;

namespace Game.Environment
{
    // TODO: make possibility to insert boss battles / treasure chests to the end of location map
    /// <summary>
    /// Класс для данных игровых мест локации. Места могут предлагать уникальные взаимодействия с картами.
    /// </summary>
    public sealed class LocationPlace
    {
        public readonly string id;
        public string name;
        public string desc;
        public float frequency;
        public string iconPath;
        public Func<PlaceMenu> menuCreator;

        public LocationPlace(string id) 
        {
            this.id = id;
            this.iconPath = $"Sprites/Places/{id.Replace('_', ' ')}";
        }
    }
}
