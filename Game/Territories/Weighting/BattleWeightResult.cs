using System;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий результат взвешивания сущности во время боя.
    /// </summary>
    public class BattleWeightResult<T> : IBattleWeightResult, IComparable<BattleWeightResult<T>>, IEquatable<BattleWeightResult<T>> where T : IBattleEntity
    {
        public T Entity => entity;
        public float WeightDeltaAbs => weightDeltaAbs;
        public float WeightDeltaRel => weightDeltaRel;

        protected float weightDeltaAbs;
        protected float weightDeltaRel;
        protected readonly T entity;
        IBattleEntity IBattleWeightResult.Entity => entity;

        public BattleWeightResult(T entity, float weightDeltaAbs, float weightDeltaRel)
        {
            this.entity = entity;
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
            return entity.Equals(other.entity);
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
