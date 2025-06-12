using Game.Territories;
using GreenOne;
using System;

namespace Game.Cards
{
    /// <summary>
    ///  ласс, представл€ющий информацию о стоимости игровой карты.
    /// </summary>
    public class CardPrice : IEquatable<CardPrice>
    {
        public CardCurrency currency;
        public int value;

        public CardPrice(CardCurrency currency, int value)
        {
            this.currency = currency;
            this.value = value;
        }

        public static bool operator ==(CardPrice left, CardPrice right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CardPrice left, CardPrice right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode() + currency.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TerritoryRange range && Equals(range);
        }
        public bool Equals(CardPrice other)
        {
            return value == other.value && currency == other.currency;
        }
    }
}