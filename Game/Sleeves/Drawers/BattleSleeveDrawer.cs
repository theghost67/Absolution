using UnityEngine;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattleSleeve"/>.
    /// </summary>
    public class BattleSleeveDrawer : TableSleeveDrawer
    {
        public readonly new BattleSleeve attached;
        public BattleSleeveDrawer(BattleSleeve sleeve, Transform parent) : base(sleeve, parent)
        {
            attached = sleeve;
            if (!attached.isForMe)
                MoveOutInstantly();
        }
        protected override bool UpdateUserInput()
        {
            return base.UpdateUserInput() && (attached.Side.Drawer?.SleeveIsVisible ?? false);
        }
    }
}
