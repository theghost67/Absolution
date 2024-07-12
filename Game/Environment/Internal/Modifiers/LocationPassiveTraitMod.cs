using Game.Traits;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий модификатор появления пассивного навыка в локации.
    /// </summary>
    public class LocationPassiveTraitMod : LocationEntityMod
    {
        public LocationPassiveTraitMod(string id, bool add = false, bool remove = false, float absValue = 0, float relValue = 1) : base(id, add, remove, absValue, relValue) { }
        public override float GetSourceValue(string id) => TraitBrowser.GetPassive(id).frequency;
    }
}
