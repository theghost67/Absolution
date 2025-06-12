namespace Game.Territories
{
    /// <summary>
    /// Реализует объект как объект сражения на территории, принадлежащий одной из сторон, который можно найти на территории и который имеет область действия.
    /// </summary>
    public interface IBattleFighter : IBattleObject, IBattleRanging
    {
    }
}
