namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий модификатор появления места в локации.
    /// </summary>
    public class LocationPlaceMod : LocationEntityMod
    {
        public LocationPlaceMod(string id, bool add = false, bool remove = false, float absValue = 0, float relValue = 1) : base(id, add, remove, absValue, relValue) { }
        public override float GetSourceValue(string id) => EnvironmentBrowser.LocationPlaces[id].frequency;
    }
}
