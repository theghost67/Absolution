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
            {
                MoveOutInstantly();
                attached.Side.Opposite.Sleeve.Drawer.OnPullOut += TryPullOut;
                attached.Side.Opposite.Sleeve.Drawer.OnPullIn += TryPullIn;
            }
        }

        public override void PullOut()
        {
            if (IsPulledOut) return;
            base.PullOut();
            if (attached.isForMe) return;
            foreach (IBattleSleeveCard card in attached)
                card.Drawer.FlipRendererY();
        }
        public override void PullIn()
        {
            if (!IsPulledOut) return;
            base.PullIn();
            if (attached.isForMe) return;
            foreach (IBattleSleeveCard card in attached)
                card.Drawer.FlipRendererY();
        }

        protected override bool UpdateUserInput()
        {
            return base.UpdateUserInput() && attached.isForMe && !IsMovedOut;
        }

        private void TryPullOut()
        {
            if (!IsMovedOut)
                PullOut();
        }
        private void TryPullIn()
        {
            if (!IsMovedOut)
                PullIn();
        }
    }
}
