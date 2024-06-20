using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BatteFloatCard"/>.
    /// </summary>
    public class BattleFloatCardDrawer : TableFloatCardDrawer
    {
        public readonly new BattleFloatCard attached;
        public BattleFloatCardDrawer(BattleFloatCard card, Transform parent) : base(card, parent)
        {
            attached = card;
        }
    }
}
