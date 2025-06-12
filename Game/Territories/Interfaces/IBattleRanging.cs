namespace Game.Territories
{
    /// <summary>
    /// Реализует объект сражения как объект, имеющий обзор на территорию сражения. 
    /// </summary>
    public interface IBattleRanging
    {
        public BattleRange Range { get; }
        public BattleArea Area { get; }
    }
}
