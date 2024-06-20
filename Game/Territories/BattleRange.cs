using GreenOne;
using System;

namespace Game.Territories
{
    /// <summary>
    /// Структура, определяющая цели какого-либо совершаемого действия во время сражения.
    /// </summary>
    public readonly struct BattleRange : IEquatable<BattleRange>, ISerializable, ICloneable
    {
        public static readonly BattleRange none = new(TerritoryRange.none, TerritoryRange.none);
        public static readonly BattleRange normal = new(TerritoryRange.oppositeSingle, TerritoryRange.ownerSingle);
        public static readonly BattleRange self = new(TerritoryRange.ownerSingle);

        public readonly TerritoryRange potential;
        public readonly TerritoryRange splash;
        public readonly int priority; // determines BattleArea target events raise order

        public BattleRange(TerritoryRange potential, TerritoryRange splash, int priority = 0)
        {
            this.potential = potential;
            this.splash = splash;
            this.priority = priority;

            CheckAimRange();
        }
        public BattleRange(TerritoryRange potential, int priority = 0)
        {
            this.potential = potential;
            this.splash = TerritoryRange.ownerSingle;
            this.priority = priority;

            CheckAimRange();
        }
        public BattleRange(SerializationDict dict)
        {
            potential = new TerritoryRange(dict.DeserializeKeyAsDict("potential"));
            splash = new TerritoryRange(dict.DeserializeKeyAsDict("splash"));
            priority = dict.DeserializeKeyAs<int>("priority");
        }

        public static bool operator ==(BattleRange left, BattleRange right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(BattleRange left, BattleRange right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public SerializationDict Serialize()
        {
            return new SerializationDict()
            {
                { "potential", potential },
                { "splash", splash },
                { "priority", priority },
            };
        }
        public override int GetHashCode()
        {
            return potential.GetHashCode() * 10 + splash.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is TerritoryRange range)
                return Equals(range);
            else return false;
        }
        public bool Equals(BattleRange other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        void CheckAimRange()
        {
            if (potential.targets == TerritoryTargets.None || potential.fields == TerritoryFields.Both)
                return;

            if (potential.fields == TerritoryFields.None)
                throw new ArgumentException("Aim range must have at least one Fields flag.");
        }
    }
}
