namespace Game.Territories
{
    /// <summary>
    /// Реализует объект сражения как объект, имеющий вес на территории сражения. 
    /// </summary>
    public interface IBattleWeighty
    {
        public BattleWeight CalculateWeight(int[] excludedWeights);
    }
}
