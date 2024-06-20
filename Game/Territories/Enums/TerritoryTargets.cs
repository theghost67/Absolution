namespace Game.Territories
{
    /// <summary>
    /// Перечисление, обозначающее количество целей на территории.
    /// </summary>
    public enum TerritoryTargets
    {
        None = 0,
        Single = 1,
        Double = 2,
        Triple = Single | Double,
        All = 4,
        NotSelf = 16,
    }
}
