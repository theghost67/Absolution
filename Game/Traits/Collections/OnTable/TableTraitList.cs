﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using GreenOne;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий список трейтов на столе (см. <see cref="ITableTraitListElement"/>).<br/>
    /// Список является частью набора списков трейтов (см. <see cref="TableTraitListSet"/>).
    /// </summary>
    public abstract class TableTraitList : ITableTraitList
    {
        public int Count => _list.Count;
        public IIdEventBoolAsync<TableTraitStacksTryArgs> OnStacksTryToChange => _onStacksTryToChange;
        public IIdEventVoidAsync<TableTraitStacksSetArgs> OnStacksChanged => _onStacksChanged;
        public TableTraitListSet Set => _set;

        readonly TableEventBool<TableTraitStacksTryArgs> _onStacksTryToChange; 
        readonly TableEventVoid<TableTraitStacksSetArgs> _onStacksChanged;
        readonly TableTraitListSet _set;
        readonly List<ITableTraitListElement> _list;

        public TableTraitList(TableTraitListSet set) 
        {
            _set = set;
            _list = new List<ITableTraitListElement>();
            _onStacksTryToChange = new TableEventBool<TableTraitStacksTryArgs>();
            _onStacksChanged = new TableEventVoid<TableTraitStacksSetArgs>();

            _onStacksTryToChange.Add(OnStacksTryToChangeBase_TOP, 256);
            _onStacksChanged.Add(OnStacksChangedBase_TOP, 256);
        }
        protected TableTraitList(TableTraitList src, TableTraitListCloneArgs args)
        {
            _set = args.srcListSetClone;
            _list = new List<ITableTraitListElement>();
            _onStacksTryToChange = (TableEventBool<TableTraitStacksTryArgs>)src._onStacksTryToChange.Clone();
            _onStacksChanged = (TableEventVoid<TableTraitStacksSetArgs>)src._onStacksChanged.Clone();

            args.AddOnClonedAction(src.GetType(), typeof(TableTraitList), () =>
            {
                foreach (ITableTraitListElement srcElement in src)
                    _list.Add(ElementCloner(srcElement, args));
            });
        }

        public ITableTraitListElement this[string id] => _list.FirstOrDefault(e => e.Trait.Data.id == id);
        public ITableTraitListElement this[int index] => index >= 0 && index < _list.Count ? _list[index] : null;
        public IEnumerator<ITableTraitListElement> GetEnumerator() => GetElements().GetEnumerator();

        public virtual void Dispose()
        {
            for (int i = _list.Count - 1; i >= 0; i--)
                this[i]?.Dispose();

            _list.Clear();
            _onStacksTryToChange.Clear();
            _onStacksChanged.Clear();
        }
        public abstract object Clone(CloneArgs args);

        // use entryId and Revert method to revert applied effect instead of calling this method with negative value
        // (as value can be modified by OnStacksTryToChange event)
        public async UniTask Adjust(string id, int stacks, ITableEntrySource source, string entryId = null)
        {
            if (stacks == 0) return;
            TableTraitStacksTryArgs listArgs = new(id, stacks, source);
            if (!await _onStacksTryToChange.InvokeAND(this, listArgs)) return;

            ITableTraitListElement element = AdjustInternal(listArgs);
            if (element == null) return;

            ITableTrait trait = element.Trait;
            TableEntry entry;

            if (source is IBattleEntity sourceInBattle)
                 entry = new TableEntry(stacks, source, sourceInBattle.Territory.Turn);
            else entry = new TableEntry(stacks, source);

            ((TableTraitListElement)element).AddEntryInternal(entryId, entry);
            TableTraitStacksSetArgs stacksArgs = new(element, stacks, source);

            if (element.Drawer?.enqueueAnims ?? false)
                _set.Drawer?.elements.Enqueue(element, stacks);

            await trait.Data.OnStacksChanged(stacksArgs);
            await _onStacksChanged.Invoke(this, stacksArgs);
        }
        public async UniTask Revert(string id, string entryId)
        {
            ITableEntryDict entries = this[id].StacksEntries;
            if (!entries.TryGetValue(entryId, out TableEntry entry))
                return;

            TableTraitStacksTryArgs listArgs = new(id, (int)-entry.value, entry.source);
            ITableTraitListElement element = AdjustInternal(listArgs);
            if (element == null) return;

            ITableTrait trait = element.Trait;
            TableTraitStacksSetArgs stacksArgs = new(element, listArgs.stacks, listArgs.source);

            if (element.Drawer?.enqueueAnims ?? false)
                _set.Drawer?.elements.Enqueue(element, listArgs.stacks);

            await trait.Data.OnStacksChanged(stacksArgs);
            await _onStacksChanged.Invoke(this, stacksArgs);
        }

        public void AdjustByOwnerList(TraitList dataList)
        {
            foreach (TraitListElement element in dataList)
                Adjust(element.Trait.id, element.Stacks, _set.Owner);
        }
        public void Clear(ITableEntrySource source)
        {
            ITableTraitListElement[] values = this.ToArray();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ITableTraitListElement element = values[i];
                Adjust(element.Trait.Data.id, -element.Stacks, source);
            }
        }

        protected IEnumerable<ITableTraitListElement> GetElements() => _list;
        protected ITableTraitListElement GetElement(string id) => this[id];
        protected ITableTraitListElement GetElement(int index) => this[index];

        protected abstract ITableTraitListElement ElementCreator(TableTraitStacksTryArgs e);
        protected abstract ITableTraitListElement ElementRemover(TableTraitStacksTryArgs e);
        protected abstract ITableTraitListElement ElementCloner(ITableTraitListElement src, TableTraitListCloneArgs args);

        protected virtual UniTask<bool> OnStacksTryToChangeBase_TOP(object sender, TableTraitStacksTryArgs e)
        {
            TableTraitList list = (TableTraitList)sender;
            TableFieldCard owner = list.Set.Owner;

            if (e.source != null)
                 owner.Field?.Territory?.WriteLogForDebug($"{owner.TableName}: навык {e.id}: попытка изменения на {e.stacks} ед. от {e.source.TableName}.");
            else owner.Field?.Territory?.WriteLogForDebug($"{owner.TableName}: навык {e.id}: попытка изменения на {e.stacks} ед.");
            return UniTask.FromResult(true);
        }
        protected virtual UniTask OnStacksChangedBase_TOP(object sender, TableTraitStacksSetArgs e)
        {
            TableTraitList list = (TableTraitList)sender;
            TableFieldCard owner = list.Set.Owner;

            if (e.source != null)
                 owner.Field?.Territory?.WriteLogForDebug($"{owner.TableName}: навык {e.Trait.Data.id}: изменение на {e.delta} ед. от {e.source.TableName}.");
            else owner.Field?.Territory?.WriteLogForDebug($"{owner.TableName}: навык {e.Trait.Data.id}: изменение на {e.delta} ед.");
            return UniTask.CompletedTask;
        }

        ITableTraitListElement AdjustInternal(TableTraitStacksTryArgs args)
        {
            if (args.stacks == 0) return null;
            ITableTraitListElement element;
            if (args.stacks > 0)
            {
                element = ElementCreator(args);
                if (element.WasAdded(args.stacks))
                    _list.Add(element);
            }
            else
            {
                element = ElementRemover(args);
                if (element != null && element.WasRemoved(args.stacks))
                    _list.Remove(element);
            }
            return element;
        }

        IEnumerable<ITableTraitListElement> ITableTraitList.GetElements() => GetElements();
        ITableTraitListElement ITableTraitList.GetElement(string id) => GetElement(id);
        ITableTraitListElement ITableTraitList.GetElement(int index) => GetElement(index);

        IEnumerator<ITableTraitListElement> IEnumerable<ITableTraitListElement>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}