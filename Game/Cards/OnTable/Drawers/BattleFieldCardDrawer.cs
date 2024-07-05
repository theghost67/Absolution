using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattleFieldCard"/>.
    /// </summary>
    public class BattleFieldCardDrawer : TableFieldCardDrawer
    {
        public readonly new BattleFieldCard attached;
        public BattleFieldCardDrawer(BattleFieldCard card, Transform parent) : base(card, parent) 
        {
            attached = card;
        }
    }
}
