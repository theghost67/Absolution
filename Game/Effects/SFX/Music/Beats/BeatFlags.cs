namespace Game.Effects
{
    /// <summary>
    /// Перечисление, содержащее некоторые особенности битов. Позволяет определить одни биты от других, применяя другие спец-эффекты.
    /// </summary>
    public enum BeatFlags
    {
        None = 0,

        PeakOne = 1,
        PeakTwo = 2,

        FlickerSwitch = 4,
        RiseSwitch = 8,
    }
}
