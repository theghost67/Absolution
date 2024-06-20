namespace Game.Territories
{
    /// <summary>
    /// Перечисление, обозначающее сторону целей на территории.
    /// </summary>
    public enum TerritoryFields
    {
        None = 0,

        Owner = 1,
        Opposite = 2,

        Both = Owner | Opposite,
    }
}
