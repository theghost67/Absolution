namespace Game.Traits
{
    /// <summary>
    /// Перечисление, представляющее тэги навыка. Помогает отделить одни навыки от других.
    /// </summary>
    public enum TraitTag
    {
        None   = 0,
        Static = 1, // non-stackable trait (stacks don't change trait effectiveness)

    }
}
