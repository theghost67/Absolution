using GreenOne;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Game.Territories
{
    /// <summary>
    /// Структура, позволяющая выделить игровые поля из всей игровой территории.
    /// </summary>
    [Serializable] public readonly struct TerritoryRange : IEquatable<TerritoryRange>, ISerializable, ICloneable
    {
        public const int MAX_WIDTH = TableTerritory.MAX_WIDTH;
        public const int MAX_HEIGHT = TableTerritory.MAX_HEIGHT;
        public const int MAX_SIZE = TableTerritory.MAX_SIZE;

        public static readonly TerritoryRange none = new(TerritoryFields.None, TerritoryTargets.None);
        public static readonly TerritoryRange all = new(TerritoryFields.Both, TerritoryTargets.All);
        public static readonly TerritoryRange allNotSelf = new(TerritoryFields.Both, TerritoryTargets.All | TerritoryTargets.NotSelf);

        public static readonly TerritoryRange defaultPotential = new(TerritoryFields.Opposite, TerritoryTargets.Single);
        public static readonly TerritoryRange defaultSplash = new(TerritoryFields.Owner, TerritoryTargets.Single);

        public static readonly TerritoryRange ownerSingle = new(TerritoryFields.Owner, TerritoryTargets.Single);
        public static readonly TerritoryRange ownerDouble = new(TerritoryFields.Owner, TerritoryTargets.Double);
        public static readonly TerritoryRange ownerTriple = new(TerritoryFields.Owner, TerritoryTargets.Triple);
        public static readonly TerritoryRange ownerAll = new(TerritoryFields.Owner, TerritoryTargets.All);
        public static readonly TerritoryRange ownerAllNotSelf = new(TerritoryFields.Owner, TerritoryTargets.All | TerritoryTargets.NotSelf);

        public static readonly TerritoryRange oppositeSingle = new(TerritoryFields.Opposite, TerritoryTargets.Single);
        public static readonly TerritoryRange oppositeDouble = new(TerritoryFields.Opposite, TerritoryTargets.Double);
        public static readonly TerritoryRange oppositeTriple = new(TerritoryFields.Opposite, TerritoryTargets.Triple);
        public static readonly TerritoryRange oppositeAll = new(TerritoryFields.Opposite, TerritoryTargets.All);

        public static readonly TerritoryRange bothSingle = new(TerritoryFields.Both, TerritoryTargets.Single);
        public static readonly TerritoryRange bothDouble = new(TerritoryFields.Both, TerritoryTargets.Double);
        public static readonly TerritoryRange bothTriple = new(TerritoryFields.Both, TerritoryTargets.Triple);

        public readonly TerritoryFields fields;
        public readonly TerritoryTargets targets;

        public readonly int fieldsModifier;
        public readonly int targetsCount;
        public readonly bool targetIsSingle;

        public TerritoryRange(TerritoryFields fields, TerritoryTargets targets)
        {
            this.fields = fields;
            this.targets = targets;

            fieldsModifier = fields == TerritoryFields.Both ? 2 : 1;
            targetIsSingle = fieldsModifier == 1 && targets == TerritoryTargets.Single;
            targetsCount = targetIsSingle ? 1 : CountTargets(targets, fieldsModifier);
        }
        public TerritoryRange(SerializationDict dict)
        {
            fields = dict.DeserializeKeyAs<TerritoryFields>("fields");
            targets = dict.DeserializeKeyAs<TerritoryTargets>("targets");

            fieldsModifier = fields == TerritoryFields.Both ? 2 : 1 ;
            targetIsSingle = fieldsModifier == 1 && targets == TerritoryTargets.Single;
            targetsCount = targetIsSingle ? 1 : CountTargets(targets, fieldsModifier);
        }

        public static bool operator ==(TerritoryRange left, TerritoryRange right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TerritoryRange left, TerritoryRange right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public override int GetHashCode()
        {
            return (int)fields * 256 + (int)targets;
        }

        public override bool Equals(object obj)
        {
            if (obj is TerritoryRange range)
                 return Equals(range);
            else return false;
        }
        public bool Equals(TerritoryRange other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public int2[] OverlapMiddlePos(bool inverted = false)
        {
            return Overlap(new int2(2, inverted ? 1 : 0));
        }
        public int2[] Overlap(int2 centerPos)
        {
            return Overlap(centerPos, pos => true);
        }
        public int2[] Overlap(int2 centerPos, Predicate<int2> filter)
        {
            List<int2> positions = new(MAX_SIZE);
            for (int x = 0; x < MAX_WIDTH; x++)
            {
                for (int y = 0; y < MAX_HEIGHT; y++)
                {
                    int2 pos = new(x, y);
                    if (!filter(pos)) continue;

                    if (!OverlapsX(centerPos.x, x)) continue;
                    if (!OverlapsY(centerPos.y, y)) continue;

                    positions.Add(pos);
                }
            }
            return positions.ToArray();
        }

        public bool OverlapsX(int centerX, int currentX)
        {
            if (targets.HasFlag(TerritoryTargets.All))
                return true;

            if (targets.HasFlag(TerritoryTargets.Single) && centerX == currentX)
                return true;

            if (targets.HasFlag(TerritoryTargets.Double) && (currentX == centerX - 1 || currentX == centerX + 1))
                return true;

            return false;
        }
        public bool OverlapsY(int centerY, int currentY)
        {
            bool selectOwnerFields = fields.HasFlag(TerritoryFields.Owner);
            bool selectOppositeFields = fields.HasFlag(TerritoryFields.Opposite);

            if (selectOwnerFields && centerY == currentY)
                return true;

            if (selectOppositeFields && centerY != currentY)
                return true;

            return false;
        }

        public SerializationDict Serialize()
        {
            return new SerializationDict()
            {
                { "fields", fields },
                { "targets", targets },
            };
        }
        static int CountTargets(TerritoryTargets targets, int fieldsMod)
        {
            if (targets.HasFlag(TerritoryTargets.All))
                return 5 * fieldsMod;

            int count = 0;
            if (targets.HasFlag(TerritoryTargets.Single))
                count += 1;

            if (targets.HasFlag(TerritoryTargets.Double))
                count += 2;

            if (targets.HasFlag(TerritoryTargets.NotSelf))
                 return count * fieldsMod - 1;
            else return count * fieldsMod;
        }
    }
}
