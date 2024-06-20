namespace Game.Traits
{
    /// <summary>
    /// Перечисление, представляющее требование к характеристике инициативы будущего владельца трейта.
    /// </summary>
    public enum TraitMoxieReq
    {
        Any,    // 1-5
        Low,    // 1-2
        Medium, // 2-4
        High,   // 4-5

        HigherIsBetter,
        LowerIsBetter,
    }
}
