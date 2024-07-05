using Cysharp.Threading.Tasks;
using Game.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий набор списков пассивных и активных трейтов на столе (см. <see cref="ITableTraitList"/>).
    /// </summary>
    public class TableTraitListSet : TableObject, IEnumerable<ITableTraitListElement>, ICloneableWithArgs
    {
        public int Count => _passives.Count + _actives.Count;
        public new TableTraitListSetDrawer Drawer => ((TableObject)this).Drawer as TableTraitListSetDrawer;
        public TableFieldCard Owner => _owner;
        public TablePassiveTraitList Passives => _passives;
        public TableActiveTraitList Actives => _actives;

        readonly TableFieldCard _owner;
        TablePassiveTraitList _passives;
        TableActiveTraitList _actives;

        // drawer creates when owner drawer is created
        public TableTraitListSet(TableFieldCard owner) : base()
        {
            _owner = owner;
            AddOnInstantiatedAction(GetType(), typeof(TableTraitListSet), () =>
            {
                _passives = PassivesCreator();
                _actives = ActivesCreator();
            });
        }
        public TableTraitListSet(TableTraitListSet src, TableTraitListSetCloneArgs args) : base(src)
        {
            _owner = args.srcSetOwnerClone;
            AddOnInstantiatedAction(GetType(), typeof(TableTraitListSet), () =>
            {
                _passives = PassivesCloner(src._passives, args);
                _actives = ActivesCloner(src._actives, args);
            });
        }

        public ITableTraitListElement this[string id] => (ITableTraitListElement)_passives[id] ?? _actives[id];
        public ITableTraitListElement this[int index] => throw new NotSupportedException($"Trait list set indexing is not supported. Use {nameof(ITableTraitList)} indexing instead.");

        public override void Dispose()
        {
            base.Dispose();
            _passives.Dispose();
            _actives.Dispose();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableTraitListSetCloneArgs cArgs)
                 return new TableTraitListSet(this, cArgs);
            else return null;
        }
        public void Clear(ITableEntrySource source)
        {
            _passives.Clear(source);
            _actives.Clear(source);
        }

        public UniTask SetStacks(Trait trait, int stacks, ITableEntrySource source, string entryId = null)
        {
            if (trait.isPassive)
                 return _passives.AdjustStacks(trait.id, stacks - _passives[trait.id]?.Stacks ?? 0, source, entryId);
            else return _actives.AdjustStacks(trait.id, stacks - _actives[trait.id]?.Stacks ?? 0, source, entryId);
        }
        public UniTask AdjustStacks(Trait trait, int stacks, ITableEntrySource source, string entryId = null)
        {
            if (trait.isPassive)
                 return _passives.AdjustStacks(trait.id, stacks, source, entryId);
            else return _actives.AdjustStacks(trait.id, stacks, source, entryId);
        }
        public void AdjustStacksInRange(IEnumerable<Trait> traits, int stacks, ITableEntrySource source, string entryId = null)
        {
            foreach (Trait trait in traits)
                AdjustStacks(trait, stacks, source, entryId);
        }
        public void AdjustStacksInRange(IEnumerable<TraitListElement> elements, ITableEntrySource source, string entryId = null)
        {
            foreach (TraitListElement element in elements)
            {
                Trait data = element.Trait;
                if (data.isPassive)
                     _passives.AdjustStacks(data.id, element.Stacks, source, entryId);
                else _actives.AdjustStacks(data.id, element.Stacks, source, entryId);
            }
        }

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableTraitListSetDrawer(this);
        }

        // next methods invoke only once in ctor:
        protected virtual TablePassiveTraitList PassivesCreator()
        {
            return new TablePassiveTraitList(this);
        }
        protected virtual TableActiveTraitList ActivesCreator()
        {
            return new TableActiveTraitList(this);
        }

        protected virtual TablePassiveTraitList PassivesCloner(TablePassiveTraitList src, TableTraitListSetCloneArgs args)
        {
            TableTraitListCloneArgs listCArgs = new(this, args.terrCArgs);
            return (TablePassiveTraitList)src.Clone(listCArgs);
        }
        protected virtual TableActiveTraitList ActivesCloner(TableActiveTraitList src, TableTraitListSetCloneArgs args)
        {
            TableTraitListCloneArgs listCArgs = new(this, args.terrCArgs);
            return (TableActiveTraitList)src.Clone(listCArgs);
        }
        // ---------

        public IEnumerator<ITableTraitListElement> GetEnumerator()
        {
            foreach (TablePassiveTraitListElement element in _passives)
                yield return element;
            foreach (TableActiveTraitListElement element in _actives)
                yield return element;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
