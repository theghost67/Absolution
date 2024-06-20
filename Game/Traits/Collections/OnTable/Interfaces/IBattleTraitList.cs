using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Интерфейс, реализующий список трейтов на столе как список трейтов во время сражения (см. <see cref="IBattleTraitListElement"/>).
    /// </summary>
    public interface IBattleTraitList : ITableTraitList, IReadOnlyList<IBattleTraitListElement>
    {
        public new BattleTraitListSet Set { get; }
        TableTraitListSet ITableTraitList.Set => Set;

        public new IBattleTraitListElement this[string id] => (IBattleTraitListElement)GetElement(id);
        public new IBattleTraitListElement this[int index] => (IBattleTraitListElement)GetElement(index);

        ITableTraitListElement ITableTraitList.this[string id] => this[id];
        ITableTraitListElement ITableTraitList.this[int index] => this[index];
        IBattleTraitListElement IReadOnlyList<IBattleTraitListElement>.this[int index] => this[index];
    }
}
