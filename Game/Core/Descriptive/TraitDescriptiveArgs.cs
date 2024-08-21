using Game.Traits;
namespace Game
{
    /// <summary>
    /// Класс, представляющий аргументы для динамического описания навыка (см. <see cref="Trait"/>, <see cref="IDescriptive"/>).
    /// </summary>
    public class TraitDescriptiveArgs : DescriptiveArgs
    {
        public readonly new Trait src;
        public int turnsDelay;
        public int stacks;

        public TraitDescriptiveArgs(string id) : base(TraitBrowser.GetTrait(id))
        {
            src = (Trait)base.src;
            turnsDelay = -1;
            stacks = 1;
        }
    }
}
