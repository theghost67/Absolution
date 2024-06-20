using Game.Traits;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий модификатор появления активного трейта в локации.
    /// </summary>
    public class LocationActiveTraitMod : LocationEntityMod
    {
        public LocationActiveTraitMod(string id, bool add = false, bool remove = false, float absValue = 0, float relValue = 1) : base(id, add, remove, absValue, relValue) { }
        public override float GetSourceValue(string id) => TraitBrowser.GetActive(id).frequency;
    }
}
