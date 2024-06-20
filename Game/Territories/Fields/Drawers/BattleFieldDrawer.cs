using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattleField"/>.
    /// </summary>
    public class BattleFieldDrawer : TableFieldDrawer
    {
        public readonly new BattleField attached;
        public BattleFieldDrawer(BattleField field, Transform parent) : base(field, parent) 
        {
            attached = field;
            if (!field.Side.isMe)
                FlipY();
        }
    }
}
