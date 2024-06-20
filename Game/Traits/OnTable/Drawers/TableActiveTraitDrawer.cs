using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableActiveTrait"/>.
    /// </summary>
    public class TableActiveTraitDrawer : TableTraitDrawer
    {
        public readonly new TableActiveTrait attached;
        protected readonly ActiveTrait attachedData;

        public TableActiveTraitDrawer(TableActiveTrait trait, Transform parent) : base(trait, parent)
        {
            attached = trait;
            attachedData = trait.Data;
        }
    }
}
