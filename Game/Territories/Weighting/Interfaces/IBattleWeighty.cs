namespace Game.Territories
{
    /// <summary>
    /// Реализует объект сражения как объект, имеющий вес на территории сражения. 
    /// </summary>
    public interface IBattleWeighty : IBattleEntity
    {
        public BattleArea Area { get; }
        public BattleRange Range { get; }
        public BattleWeight Weight { get; }
    }
}
