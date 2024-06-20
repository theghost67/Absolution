using System.Collections.Generic;

namespace Game.Environment
{
    /// <summary>
    /// Абстрактный класс, представляющий модификатор появления какой-либо сущности в локации.
    /// </summary>
    public abstract class LocationEntityMod
    {
        public readonly string id;
        public bool add;
        public bool remove;
        public float absValue;
        public float relValue;

        public LocationEntityMod(string id, bool add = false, bool remove = false, float absValue = 0, float relValue = 1)
        {
            this.id = id;
            this.add = add;
            this.remove = remove;
            this.absValue = absValue;
            this.relValue = relValue;
        }
        public void ModifyCollection(Dictionary<string, float> collection)
        {
            if (add && remove)
                throw new System.InvalidOperationException("You must choose between 'add' and 'remove' parameter.");

            bool hasKey = collection.ContainsKey(id);
            if (remove && hasKey)
            {
                collection.Remove(id);
                return;
            }

            float newValue = (GetSourceValue(id) + absValue) * relValue;
            if (add && !hasKey)
                collection.Add(id, newValue);
            else collection[id] = newValue;
        }
        public abstract float GetSourceValue(string id);
    }
}
