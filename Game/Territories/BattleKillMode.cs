namespace Game.Cards
{
    /// <summary>
    /// Содержит способы убийства карты поля во время сражения (см. <see cref="BattleFieldCard"/>).
    /// </summary>
    public enum BattleKillMode
    {
        Default = 0,
        IgnoreHealthRestore = 1,
        IgnoreCanBeKilled = 2,
        IgnoreEverything = IgnoreHealthRestore | IgnoreCanBeKilled,
    }
}
