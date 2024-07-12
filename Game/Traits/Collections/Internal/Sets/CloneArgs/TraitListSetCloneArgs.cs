using Game.Cards;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования набора списков навыков на столе.
    /// </summary>
    public class TraitListSetCloneArgs : CloneArgs
    {
        public readonly FieldCard srcSetOwnerClone;
        public TraitListSetCloneArgs(FieldCard srcSetOwnerClone)
        {
            this.srcSetOwnerClone = srcSetOwnerClone;
        }
    }
}
