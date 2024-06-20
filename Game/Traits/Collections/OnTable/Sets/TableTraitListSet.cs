using Cysharp.Threading.Tasks;
using Game.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий набор списков пассивных и активных трейтов на столе (см. <see cref="ITableTraitList"/>).
    /// </summary>
    public class TableTraitListSet : ITableDrawable, IEnumerable<ITableTraitListElement>, ICloneableWithArgs, IDisposable
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public int Count => _passives.Count + _actives.Count;
        public TableFieldCard Owner => _owner;
        public TablePassiveTraitList Passives => _passives;
        public TableActiveTraitList Actives => _actives;
        public TableTraitListSetDrawer Drawer => _drawer;
        Drawer ITableDrawable.Drawer => _drawer;

        readonly TableFieldCard _owner;
        TablePassiveTraitList _passives;
        TableActiveTraitList _actives;
        TableTraitListSetDrawer _drawer;

        public TableTraitListSet(TableFieldCard owner) 
        {
            _owner = owner;
            _passives = PassivesCreator();
            _actives = ActivesCreator();
        }
        public TableTraitListSet(TableTraitListSet src, TableTraitListSetCloneArgs args)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            _owner = args.srcSetOwnerClone;
            args.AddOnClonedAction(src.GetType(), typeof(TableTraitListSet), () =>
            {
                _passives = PassivesCloner(src._passives, args);
                _actives = ActivesCloner(src._actives, args);
            });
        }

        public ITableTraitListElement this[string id] => (ITableTraitListElement)_passives[id] ?? _actives[id];
        public ITableTraitListElement this[int index] => throw new NotSupportedException($"Trait list set indexing is not supported. Use {nameof(ITableTraitList)} indexing instead.");

        public void Dispose()
        {
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
        public UniTask Adjust(Trait trait, int stacks, ITableEntrySource source)
        {
            if (trait.isPassive)
                 return _passives.Adjust(trait.id, stacks, source);
            else return _actives.Adjust(trait.id, stacks, source);
        }
        public void AdjustRange(IEnumerable<Trait> traits, int stacks, ITableEntrySource source)
        {
            foreach (Trait trait in traits)
                Adjust(trait, stacks, source);
        }
        public void AdjustRange(IEnumerable<TraitListElement> elements, ITableEntrySource source)
        {
            foreach (TraitListElement element in elements)
            {
                Trait data = element.Trait;
                if (data.isPassive)
                     _passives.Adjust(data.id, element.Stacks, source);
                else _actives.Adjust(data.id, element.Stacks, source);
            }
        }

        public void CreateDrawer(Transform parent)
        {
            _drawer ??= new TableTraitListSetDrawer(this);
            OnDrawerCreated?.Invoke(this, EventArgs.Empty);
        }
        public void DestroyDrawer(bool instantly)
        {
            _drawer?.TryDestroy(instantly);
            _drawer = null;
            OnDrawerDestroyed?.Invoke(this, EventArgs.Empty);
        }

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
    }
}
