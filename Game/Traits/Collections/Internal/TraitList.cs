using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий список данных навыков (см. <see cref="TraitListElement"/>).<br/>
    /// Список является частью набора списков навыков (см. <see cref="TraitListSet"/>).
    /// </summary>
    public abstract class TraitList : ITraitList
    {
        public int Count => _list.Count;
        public TraitListSet Set => _set;

        readonly TraitListSet _set;
        readonly List<TraitListElement> _list;

        public TraitList(TraitListSet set)
        {
            _set = set;
            _list = new List<TraitListElement>();
        }
        protected TraitList(TraitList src, TraitListCloneArgs args)
        {
            _set = args.srcListSetClone;
            _list = new List<TraitListElement>();
            args.AddOnClonedAction(src.GetType(), typeof(TraitList), () => CloneElements(src, args));
        }

        public TraitListElement this[string id] => _list.FirstOrDefault(e => e.Trait.id == id);
        public TraitListElement this[int index] => index >= 0 && index < _list.Count ? _list[index] : null;
        public IEnumerator<TraitListElement> GetEnumerator() => GetElements().GetEnumerator();

        public abstract object Clone(CloneArgs args);

        public bool SetStacks(string id, int stacks)
        {
            TraitListElement element = this[id];
            return AdjustStacks(id, stacks - element?.Stacks ?? 0);
        }
        public bool AdjustStacks(string id, int stacks)
        {
            if (stacks == 0) return false;
            TraitListElement element;
            TraitListEventArgs listArgs = new(id, stacks);

            if (stacks > 0)
            {
                element = ElementCreator(listArgs);
                if (element.WasAdded(stacks))
                    _list.Add(element);
            }
            else
            {
                element = ElementRemover(listArgs);
                if (element == null) return false;
                if (element.WasRemoved(stacks))
                    _list.Remove(element);
            }
            return true;
        }
        public void AdjustStacksByOwnerList(TraitList dataList)
        {
            foreach (TraitListElement element in dataList)
                AdjustStacks(element.Trait.id, element.Stacks);
        }
        public void Clear()
        {
            TraitListElement[] values = this.ToArray();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                TraitListElement element = values[i];
                AdjustStacks(element.Trait.id, -element.Stacks);
            }
        }

        protected IEnumerable<TraitListElement> GetElements() => _list;
        protected TraitListElement GetElement(string id) => this[id];
        protected TraitListElement GetElement(int index) => this[index];

        protected abstract TraitListElement ElementCreator(TraitListEventArgs e);
        protected abstract TraitListElement ElementRemover(TraitListEventArgs e);

        protected abstract TraitListElement ElementCloner(TraitListElement src, TraitListCloneArgs args);
        protected void CloneElements(TraitList src, TraitListCloneArgs args)
        {
            foreach (TraitListElement srcElement in src)
                _list.Add(ElementCloner(srcElement, args));
        }

        IEnumerable<TraitListElement> ITraitList.GetElements() => GetElements();
        TraitListElement ITraitList.GetElement(string id) => GetElement(id);
        TraitListElement ITraitList.GetElement(int index) => GetElement(index);

        IEnumerator<TraitListElement> IEnumerable<TraitListElement>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
