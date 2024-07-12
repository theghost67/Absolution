namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования списка данных навыков.
    /// </summary>
    public class TraitListCloneArgs : CloneArgs
    {
        public readonly TraitListSet srcListSetClone;
        public TraitListCloneArgs(TraitListSet srcListSetClone)
        {
            this.srcListSetClone = srcListSetClone;
        }
    }
}
