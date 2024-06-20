using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TablePassiveTrait"/>.
    /// </summary>
    public class TablePassiveTraitDrawer : TableTraitDrawer
    {
        public readonly new TablePassiveTrait attached;
        protected readonly PassiveTrait attachedData;

        public TablePassiveTraitDrawer(TablePassiveTrait trait, Transform parent) : base(trait, parent)
        {
            attached = trait;
            attachedData = trait.Data;
        }
    }
}
