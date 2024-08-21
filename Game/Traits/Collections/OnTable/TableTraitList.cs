using Cysharp.Threading.Tasks;
using Game.Cards;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий список навыков на столе (см. <see cref="ITableTraitListElement"/>).<br/>
    /// Список является частью набора списков навыков (см. <see cref="TableTraitListSet"/>).
    /// </summary>
    public abstract class TableTraitList : TableObject, ITableTraitList
    {
        public int Count => _list.Count;
        public IIdEventBoolAsync<TableTraitStacksTryArgs> OnStacksTryToChange => _onStacksTryToChange;
        public ITableEventVoid<TableTraitStacksSetArgs> OnStacksChanged => _onStacksChanged;
        public TableTraitListSet Set => _set;

        readonly TableTraitListSet _set;
        readonly List<ITableTraitListElement> _list;
        readonly TableEventBool<TableTraitStacksTryArgs> _onStacksTryToChange; 
        readonly TableEventVoid<TableTraitStacksSetArgs> _onStacksChanged;
        readonly string _eventsGuid;

        public TableTraitList(TableTraitListSet set) : base()
        {
            _set = set;
            _list = new List<ITableTraitListElement>();
            _onStacksTryToChange = new TableEventBool<TableTraitStacksTryArgs>();
            _onStacksChanged = new TableEventVoid<TableTraitStacksSetArgs>();
            _eventsGuid = this.GuidGen(1);

            _onStacksTryToChange.Add(_eventsGuid, OnStacksTryToChangeBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onStacksChanged.Add(_eventsGuid, OnStacksChangedBase_TOP, TableEventVoid.TOP_PRIORITY);

            TryOnInstantiatedAction(GetType(), typeof(TableTraitList));
        }
        protected TableTraitList(TableTraitList src, TableTraitListCloneArgs args)
        {
            _set = args.srcListSetClone;
            _list = new List<ITableTraitListElement>();
            _onStacksTryToChange = (TableEventBool<TableTraitStacksTryArgs>)src._onStacksTryToChange.Clone();
            _onStacksChanged = (TableEventVoid<TableTraitStacksSetArgs>)src._onStacksChanged.Clone();

            AddOnInstantiatedAction(GetType(), typeof(TableTraitList), () =>
            {
                foreach (ITableTraitListElement srcElement in src)
                    _list.Add(ElementCloner(srcElement, args));
            });
        }

        public ITableTraitListElement this[string id] => _list.FirstOrDefault(e => e.Trait.Data.id == id);
        public ITableTraitListElement this[int index] => index >= 0 && index < _list.Count ? _list[index] : null;
        public IEnumerator<ITableTraitListElement> GetEnumerator() => GetElements().GetEnumerator();

        public override void Dispose()
        {
            base.Dispose();
            for (int i = _list.Count - 1; i >= 0; i--)
                this[i]?.Dispose();

            _list.Clear();
            _onStacksTryToChange.Clear();
            _onStacksChanged.Clear();
        }
        public abstract object Clone(CloneArgs args);

        // use entryId and Revert method to revert applied effect instead of calling this method with negative value
        // (as value can be modified by OnStacksTryToChange event)

        public UniTask SetStacks(string id, int stacks, ITableEntrySource source, string entryId = null)
        {
            ITableTraitListElement element = this[id];
            return AdjustStacks(id, stacks - element?.Stacks ?? 0, source, entryId);
        }
        public async UniTask AdjustStacks(string id, int stacks, ITableEntrySource source, string entryId = null)
        {
            if (stacks == 0 || !_set.CanAdjustStacks()) return;
            TableTraitStacksTryArgs listArgs = new(id, stacks, source);
            if (!await _onStacksTryToChange.InvokeAND(this, listArgs)) return;
            if (stacks == 0 || !_set.CanAdjustStacks()) return;

            stacks = listArgs.delta;
            ITableTraitListElement element = AdjustInternal(listArgs);
            if (element == null) return;

            ITableTrait trait = element.Trait;
            TableTraitStacksSetArgs stacksArgs = new(element, stacks, source);

            if (element.Drawer != null)
            {
                if (element.Drawer.enqueueAnims)
                    _set.Drawer?.queue.Enqueue(element, stacks);
                else element.Drawer.AnimAdjust(element.Stacks);
            }

            await trait.Data.OnStacksChanged(stacksArgs);
            await ElementAwaiter(element);
            await _onStacksChanged.Invoke(this, stacksArgs);
        }
        public async UniTask RevertStacks(string id, string entryId)
        {
            ITableEntryDict entries = this[id].StacksEntries;
            if (!entries.TryGetValue(entryId, out TableEntry entry))
                return;

            TableTraitStacksTryArgs listArgs = new(id, (int)-entry.value, entry.source);
            ITableTraitListElement element = AdjustInternal(listArgs);
            if (element == null) return;

            ITableTrait trait = element.Trait;
            TableTraitStacksSetArgs stacksArgs = new(element, listArgs.delta, listArgs.source);

            if (element.Drawer != null)
            {
                if (element.Drawer.enqueueAnims)
                    _set.Drawer?.queue.Enqueue(element, listArgs.delta);
                else element.Drawer.AnimAdjust(listArgs.delta);
            }

            await trait.Data.OnStacksChanged(stacksArgs);
            await ElementAwaiter(element);
            await _onStacksChanged.Invoke(this, stacksArgs);
        }

        public async UniTask AdjustStacksByOwnerList(TraitList dataList)
        {
            foreach (TraitListElement element in dataList)
                await AdjustStacks(element.Trait.id, element.Stacks, _set.Owner);
        }
        public async UniTask Clear(ITableEntrySource source)
        {
            ITableTraitListElement[] values = this.ToArray();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ITableTraitListElement element = values[i];
                await AdjustStacks(element.Trait.Data.id, -element.Stacks, source);
            }
        }

        protected IEnumerable<ITableTraitListElement> GetElements() => _list;
        protected ITableTraitListElement GetElement(string id) => this[id];
        protected ITableTraitListElement GetElement(int index) => this[index];

        protected abstract ITableTraitListElement ElementCreator(TableTraitStacksTryArgs e);
        protected abstract ITableTraitListElement ElementRemover(TableTraitStacksTryArgs e);
        protected abstract ITableTraitListElement ElementCloner(ITableTraitListElement src, TableTraitListCloneArgs args);
        protected virtual UniTask ElementAwaiter(ITableTraitListElement element)
        {
            return UniTask.CompletedTask; // used to await trait initializing (i.e.: battle trait area observing)
        }

        protected virtual UniTask<bool> OnStacksTryToChangeBase_TOP(object sender, TableTraitStacksTryArgs e)
        {
            TableTraitList list = (TableTraitList)sender;
            TableFieldCard owner = list.Set.Owner;

            string ownerName = owner.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{ownerName}: traits: {e.id}: OnTryToChange: delta: {e.delta} (by: {sourceName}).");
            return UniTask.FromResult(true);
        }
        protected virtual UniTask OnStacksChangedBase_TOP(object sender, TableTraitStacksSetArgs e)
        {
            TableTraitList list = (TableTraitList)sender;
            TableFieldCard owner = list.Set.Owner;

            string ownerName = owner.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{ownerName}: traits: {e.trait.Data.id}: OnChanged: delta: {e.delta} (by: {sourceName}).");
            return UniTask.CompletedTask;
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            throw new System.NotSupportedException($"Trait list does not have it's own drawer. Use {nameof(_set.Drawer)} instead.");
        }

        ITableTraitListElement AdjustInternal(TableTraitStacksTryArgs args)
        {
            if (args.delta == 0) return null;
            ITableTraitListElement element;
            if (args.delta > 0)
            {
                element = ElementCreator(args);
                if (element.WasAdded(args.delta))
                    _list.Add(element);
            }
            else
            {
                element = ElementRemover(args);
                if (element != null && element.WasRemoved(args.delta))
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
