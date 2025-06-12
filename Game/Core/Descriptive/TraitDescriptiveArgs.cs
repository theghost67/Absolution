using Game.Traits;
namespace Game
{
    /// <summary>
    /// Класс, представляющий аргументы для динамического описания навыка (см. <see cref="Trait"/>, <see cref="IDescriptive"/>).
    /// </summary>
    public class TraitDescriptiveArgs : DescriptiveArgs
    {
        public readonly new Trait data;
        public readonly new ITableTrait table;
        public int turnsDelay;
        public int stacks;

        public TraitDescriptiveArgs(ITableTrait tableTrait) : base(tableTrait.Data, tableTrait)
        {
            data = (Trait)base.data;
            table = tableTrait;
            turnsDelay = -1;
            stacks = 1;
        }
        public TraitDescriptiveArgs(string id) : base(TraitBrowser.GetTrait(id), null)
        {
            data = (Trait)base.data;
            table = null;
            turnsDelay = -1;
            stacks = 1;
        }
    }
}
