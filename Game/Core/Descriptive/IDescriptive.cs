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
        public object[] DescCustomParams(); // can be used whatever you like in DescDynamic function
    }
}
