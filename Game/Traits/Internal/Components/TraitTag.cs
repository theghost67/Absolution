namespace Game.Traits
{
    /// <summary>
    /// Перечисление, представляющее тэги навыка.
    /// </summary>
    public enum TraitTag
    {
        None   = 0,
        Static = 1, // non-stackable trait (stacks don't change trait effectiveness)
        Debuff = 2,
    }
}
