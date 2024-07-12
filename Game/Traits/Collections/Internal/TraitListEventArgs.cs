namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для делегатов списка данных навыков при изменении/попытке изменения стаков у навыка.
    /// </summary>
    public class TraitListEventArgs
    {
        public readonly string id;
        public readonly int stacks;

        public TraitListEventArgs(string id, int stacks)
        {
            this.id = id;
            this.stacks = stacks;
        }
    }
}
