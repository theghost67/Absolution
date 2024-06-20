using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий тип валюты, использующийся в стоимости карты (см <see cref="CardPrice"/>).
    /// </summary>
    public abstract class CardCurrency
    {
        public readonly string id;
        public string name;
        public string desc;
        public string iconPath;
        public Color color;

        public CardCurrency(string id) { this.id = id; }

        public override bool Equals(object obj)
        {
            return obj is CardCurrency cur && cur.id == id;
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        public override string ToString()
        {
            return $"{id} ({GetType()})";
        }
    }
}