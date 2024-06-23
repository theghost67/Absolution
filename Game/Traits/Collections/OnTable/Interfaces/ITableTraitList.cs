using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Интерфейс, реализующий объект как список трейтов на столе (см. <see cref="ITableTraitListElement"/>).
    /// </summary>
    public interface ITableTraitList : IReadOnlyList<ITableTraitListElement>, ICloneableWithArgs, IDisposable
    {
        public IIdEventBoolAsync<TableTraitStacksTryArgs> OnStacksTryToChange { get; } // before trait added/removed (can be canceled)
        public IIdEventVoidAsync<TableTraitStacksSetArgs> OnStacksChanged { get; }   // after trait added/removed
        public TableTraitListSet Set { get; }

        public ITableTraitListElement this[string id] => GetElement(id);
        public new ITableTraitListElement this[int index] => GetElement(index);

        public UniTask SetStacks(string id, int stacks, ITableEntrySource source, string entryId);
        public UniTask AdjustStacks(string id, int stacks, ITableEntrySource source, string entryId);
        public UniTask RevertStacks(string id, string entryId);

        public void AdjustStacksByOwnerList(TraitList dataList);
        public void Clear(ITableEntrySource source);

        ITableTraitListElement IReadOnlyList<ITableTraitListElement>.this[int index] => this[index];

        protected IEnumerable<ITableTraitListElement> GetElements();
        protected ITableTraitListElement GetElement(string id);
        protected ITableTraitListElement GetElement(int index);
    }
}
