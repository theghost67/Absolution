using Game.Territories;
using GreenOne;
using System;

namespace Game.Cards
{
    /// <summary>
    /// Структура, содержащая информацию о стоимости игровой карты.
    /// </summary>
    public struct CardPrice : IEquatable<CardPrice>, ISerializable
    {
        public CardCurrency currency;
        public int value;

        public CardPrice(CardCurrency currency, int value)
        {
            this.currency = currency;
            this.value = value;
        }
        public CardPrice(SerializationDict dict)
        {
            currency = dict.DeserializeKeyAs<CardCurrency>("currency");
            value = dict.DeserializeKeyAs<int>("value");
        }

        public static bool operator ==(CardPrice left, CardPrice right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CardPrice left, CardPrice right)
        {
            return !left.Equals(right);
        }

        public readonly SerializationDict Serialize()
        {
            return new SerializationDict()
            {
                { "currency", currency },
                { "value", value },
            };
        }
        public readonly override int GetHashCode()
        {
            return value.GetHashCode() + currency.GetHashCode();
        }

        public readonly override bool Equals(object obj)
        {
            return obj is TerritoryRange range && Equals(range);
        }
        public readonly bool Equals(CardPrice other)
        {
            return value == other.value && currency == other.currency;
        }
    }
}