namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для делегатов списка данных трейтов при изменении/попытке изменения стаков у трейта.
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
