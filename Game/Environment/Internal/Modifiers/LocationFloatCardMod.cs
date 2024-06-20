using Game.Cards;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий модификатор появления карты без характеристик в локации.
    /// </summary>
    public class LocationFloatCardMod : LocationEntityMod
    {
        public LocationFloatCardMod(string id, bool add = false, bool remove = false, float absValue = 0, float relValue = 1) : base(id, add, remove, absValue, relValue) { }
        public override float GetSourceValue(string id) => CardBrowser.GetFloat(id).frequency;
    }
}
