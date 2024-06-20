﻿using System;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как список данных трейтов (см. <see cref="TraitListElement"/>).<br/>
    /// Список является частью набора списков трейтов (см. <see cref="TraitListSet"/>).
    /// </summary>
    public interface ITraitList : IReadOnlyList<TraitListElement>, ICloneableWithArgs
    {
        public TraitListElement this[string id] => GetElement(id);
        public new TraitListElement this[int index] => GetElement(index);

        TraitListElement IReadOnlyList<TraitListElement>.this[int index] => this[index];

        public bool Adjust(string id, int stacks);
        public void AdjustByOwnerList(TraitList dataList);
        public void Clear();

        public abstract IEnumerable<TraitListElement> GetElements();
        public abstract TraitListElement GetElement(string id);
        public abstract TraitListElement GetElement(int index);
    }
}
