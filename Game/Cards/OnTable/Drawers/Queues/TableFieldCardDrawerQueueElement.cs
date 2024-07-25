using Cysharp.Threading.Tasks;
using Game.Traits;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий одну из возможных анимаций для очереди <see cref="TableFieldCardDrawerQueue"/>.
    /// </summary>
    public abstract class TableFieldCardDrawerQueueElement
    {
        public readonly ITableTrait trait;
        public readonly ITableTraitListElement traitInList;
        public TableFieldCardDrawerQueueElement(ITableTrait trait)
        { 
            this.trait = trait; 
            this.traitInList = trait.Owner.Traits[trait.Data.id];
        }

        public abstract GameObject CreateAnimationPrefab();
        public abstract UniTask PlayAnimation(GameObject prefab);
    }
}
