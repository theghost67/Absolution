using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TablePassiveTraitListElement"/>.
    /// </summary>
    public class TablePassiveTraitListElementDrawer : TableTraitListElementDrawer
    {
        public readonly new TablePassiveTraitListElement attached;
        public TablePassiveTraitListElementDrawer(TablePassiveTraitListElement element, Transform parent) : base(element, parent) 
        {
            attached = element;
        }
    }
}
