using System;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий результат взвешивания какого-либо действия относительно сущности типа <typeparamref name="T"/> во время боя.
    /// </summary>
    public class BattleWeightResult<T> : IBattleWeightResult, IComparable<BattleWeightResult<T>>, IEquatable<BattleWeightResult<T>> where T : IBattleObject
    {
        public T Entity => entity;
        public BattleField Field => field;
        public float WeightDeltaAbs => weightDeltaAbs;
        public float WeightDeltaRel => weightDeltaRel;

        protected readonly T entity;
        protected readonly BattleField field;
        protected float weightDeltaAbs;
        protected float weightDeltaRel;
        IBattleObject IBattleWeightResult.Entity => entity;

        public BattleWeightResult(T entity, BattleField field, float weightDeltaAbs, float weightDeltaRel)
        {
            this.entity = entity;
            this.field = field;
            this.weightDeltaAbs = weightDeltaAbs;
            this.weightDeltaRel = weightDeltaRel;
        }

        public override bool Equals(object obj)
        {
            if (obj is BattleWeightResult<T> other)
                 return Equals(other);
            else return this == null;
        }
        public bool Equals(BattleWeightResult<T> other)
        {
            return GetHashCode().Equals(other.GetHashCode());
        }

        public int CompareTo(BattleWeightResult<T> other)
        {
            float delta = weightDeltaAbs - other.weightDeltaAbs;
            if (delta > 0)
                return 1;
            else return -1;
        }
        public int CompareTo(object obj)
        {
            if (obj is BattleWeightResult<T> res)
                 return CompareTo(res);
            else return -1;
        }

        public override int GetHashCode()
        {
            return entity.Guid;
        }
        public override string ToString()
        {
            return $"({weightDeltaAbs}, {weightDeltaRel}) ({GetType()})";
        }
    }
}
