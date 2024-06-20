using Game.Cards;
using GreenOne;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, содержащий списки данных активных и пассивных трейтов (см. <see cref="Trait"/>).
    /// </summary>
    public class TraitListSet : ICloneableWithArgs, ISerializable, IEnumerable<TraitListElement>
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
        public TraitListSet(FieldCard owner, TraitListElement[] traits) : this(owner) { AdjustRange(traits); }
        public TraitListSet(SerializationDict dict)
        {
            // TODO: implement
            throw new NotImplementedException();
        }
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
        public SerializationDict Serialize()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _passives.Clear();
            _actives.Clear();
        }
        public bool Adjust(Trait trait, int stacks)
        {
            if (trait.isPassive)
                 return _passives.Adjust(trait.id, stacks);
            else return _actives.Adjust(trait.id, stacks);
        }
        public void AdjustRange(IEnumerable<Trait> traits, int stacks)
        {
            foreach (Trait trait in traits)
                Adjust(trait, stacks);
        }
        public void AdjustRange(IEnumerable<TraitListElement> elements)
        {
            foreach (TraitListElement element in elements)
            {
                Trait data = element.Trait;
                if (data.isPassive)
                    _passives.Adjust(data.id, element.Stacks);
                else _actives.Adjust(data.id, element.Stacks);
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
