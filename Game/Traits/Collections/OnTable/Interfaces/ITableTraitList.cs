using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Интерфейс, реализующий объект как список навыков на столе (см. <see cref="ITableTraitListElement"/>).
    /// </summary>
    public interface ITableTraitList : ITableObject, IReadOnlyList<ITableTraitListElement>, ICloneableWithArgs
    {
        public IIdEventBoolAsync<TableTraitStacksTryArgs> OnStacksTryToChange { get; } // before trait added/removed (can be canceled)
        public ITableEventVoid<TableTraitStacksSetArgs> OnStacksChanged { get; }   // after trait added/removed
        public TableTraitListSet Set { get; }

        public ITableTraitListElement this[string id] => GetElement(id);
        public new ITableTraitListElement this[int index] => GetElement(index);

        public UniTask SetStacks(string id, int stacks, ITableEntrySource source, string entryId);
        public UniTask AdjustStacks(string id, int stacks, ITableEntrySource source, string entryId);
        public UniTask RevertStacks(string id, string entryId);

        public UniTask AdjustStacksByOwnerList(TraitList dataList);
        public UniTask Clear(ITableEntrySource source);

        ITableTraitListElement IReadOnlyList<ITableTraitListElement>.this[int index] => this[index];

        protected IEnumerable<ITableTraitListElement> GetElements();
        protected ITableTraitListElement GetElement(string id);
        protected ITableTraitListElement GetElement(int index);
    }
}
