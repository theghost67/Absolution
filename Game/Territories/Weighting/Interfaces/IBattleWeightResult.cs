using System;

namespace Game.Territories
{
    /// <summary>
    /// Реализует объект как результат взвешивания сущности во время боя.
    /// </summary>
    public interface IBattleWeightResult : IComparable
    {
        public IBattleEntity Entity { get; }
        public float WeightDeltaAbs { get; }
        public float WeightDeltaRel { get; }
    }
}
