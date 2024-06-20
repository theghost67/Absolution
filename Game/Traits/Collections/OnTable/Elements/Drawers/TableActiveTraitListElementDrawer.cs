using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableActiveTraitListElement"/>.
    /// </summary>
    public class TableActiveTraitListElementDrawer : TableTraitListElementDrawer
    {
        public readonly new TableActiveTraitListElement attached;
        public TableActiveTraitListElementDrawer(TableActiveTraitListElement element, Transform parent) : base(element, parent) 
        {
            attached = element;
        }

        protected override bool ChangePointerBase() => true;
    }
}
