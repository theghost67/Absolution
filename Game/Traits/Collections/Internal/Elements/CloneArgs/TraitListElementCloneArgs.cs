namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования элемента списка данных трейтов.
    /// </summary>
    public class TraitListElementCloneArgs : CloneArgs
    {
        public readonly TraitList srcListClone;
        public TraitListElementCloneArgs(TraitList srcListClone)
        {
            this.srcListClone = srcListClone;
        }
    }
}
