﻿using Game.Cards;
using GreenOne;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, содержащий списки данных активных и пассивных навыков (см. <see cref="Trait"/>).
    /// </summary>
    public class TraitListSet : ICloneableWithArgs, IEnumerable<TraitListElement>
    {
        public int Count => _passives.Count + _actives.Count;
        public FieldCard Owner => _owner;
        public PassiveTraitList Passives => _passives;
        public ActiveTraitList Actives => _actives;

        readonly FieldCard _owner;
        readonly PassiveTraitList _passives;
        readonly ActiveTraitList _actives;

        public TraitListSet(FieldCard owner)
        {
            _owner = owner;
            _passives = new PassiveTraitList(this);
            _actives = new ActiveTraitList(this);
        }
        public TraitListSet(FieldCard owner, TraitListElement[] traits) : this(owner) { AdjustStacksInRange(traits); }
        protected TraitListSet(TraitListSet src, TraitListSetCloneArgs args)
        {
            _owner = args.srcSetOwnerClone;

            TraitListCloneArgs listCArgs = new(this);
            _passives = (PassiveTraitList)src._passives.Clone(listCArgs);
            _actives = (ActiveTraitList)src._actives.Clone(listCArgs);
        }

        public TraitListElement this[string id] => (TraitListElement)_passives[id] ?? _actives[id];
        public TraitListElement this[int index] => throw new NotSupportedException($"Trait list set indexing is not supported. Use {nameof(TraitList)} indexing instead.");

        public object Clone(CloneArgs args)
        {
            if (args is TraitListSetCloneArgs cArgs)
                 return new TraitListSet(this, cArgs);
            else return null;
        }
        public void Clear()
        {
            _passives.Clear();
            _actives.Clear();
        }

        public bool SetStacks(string id, int stacks)
        {
            return SetStacks(TraitBrowser.GetTrait(id), stacks);
        }
        public bool AdjustStacks(string id, int stacks)
        {
            return AdjustStacks(TraitBrowser.GetTrait(id), stacks);
        }

        public bool SetStacks(Trait trait, int stacks)
        {
            if (trait.isPassive)
                return _passives.AdjustStacks(trait.id, stacks - _passives[trait.id]?.Stacks ?? 0);
            else return _actives.AdjustStacks(trait.id, stacks - _actives[trait.id]?.Stacks ?? 0);
        }
        public bool AdjustStacks(Trait trait, int stacks)
        {
            if (trait.isPassive)
                 return _passives.AdjustStacks(trait.id, stacks);
            else return _actives.AdjustStacks(trait.id, stacks);
        }

        public void AdjustStacksInRange(IEnumerable<Trait> traits, int stacks)
        {
            foreach (Trait trait in traits)
                AdjustStacks(trait, stacks);
        }
        public void AdjustStacksInRange(IEnumerable<TraitListElement> elements)
        {
            foreach (TraitListElement element in elements)
            {
                Trait data = element.Trait;
                if (data.isPassive)
                    _passives.AdjustStacks(data.id, element.Stacks);
                else _actives.AdjustStacks(data.id, element.Stacks);
            }
        }

        public IEnumerator<TraitListElement> GetEnumerator()
        {
            foreach (TraitListElement element in _passives)
                yield return element;
            foreach (TraitListElement element in _actives)
                yield return element;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
