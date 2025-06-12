namespace Game
{
    /// <summary>
    /// Реализует объект как объект с динамическим описанием.
    /// </summary>
    public interface IDescriptive
    {
        // special characters: ≤ ≥ « »
        public string DescDynamic(DescriptiveArgs args);
        public DescLinkCollection DescLinks(DescriptiveArgs args);
        public bool HasTableEquivalent(); // if true, you'll need to pass tableObject to DescriptiveArgs, thus you can create descriptions based on object state
    }
}
