namespace Game.Environment
{
    // TODO: implement stage saves
    /// <summary>
    /// Класс для данных локаций игрового мира. У них могут быть уникальные карты и места.
    /// </summary>
    public sealed class Location
    {
        public const int COUNT = 10;
        public const int COUNT_FINISHED = 3; // TODO: remove when finished all + replace with Set.Count in WorldMenu constructor

        public readonly int level;
        public readonly string id;

        public string name;
        public string iconPath;
        public int stage; // TODO: implement

        // contains id's which can be found in this location
        public string[] fieldCards;
        public string[] floatCards;
        public string[] places;

        public Location(string id, int level)
        {
            this.id = id;
            this.level = level;
            this.iconPath = $"Sprites/Locations/{id.Replace('_', ' ')}/Icon";
        }
    }
}
