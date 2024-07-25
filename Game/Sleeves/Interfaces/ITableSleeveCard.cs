using DG.Tweening;
using Game.Cards;
using Game.Territories;

namespace Game.Sleeves
{
    // TODO: recreate as ITableSleeveCardDrawer?
    /// <summary>
    /// Реализует карту стола как карту рукава, добавляя возможность подбирать, возвращать и бросать карту на поле из рукава.<br/>
    /// </summary>
    public interface ITableSleeveCard : ITableCard
    {
        public const float PULL_DURATION = 0.33f;
        public TableSleeve Sleeve { get; }
        private static readonly System.Exception _ex = new($"{nameof(ITableSleeveCard)} methods (except for {nameof(TryDropOn)}) should be invoked only by player (user) interaction and when the card has it's own {nameof(Drawer)} ({nameof(Sleeve)} must have drawer too).");

        public bool TryTake()
        {
            if (Drawer == null)
                throw _ex;

            if (CanTake())
            {
                OnTake();
                return true;
            }
            else return false;
        }
        public bool TryReturn()
        {
            if (Drawer == null)
                throw _ex;

            if (CanReturn())
            {
                OnReturn();
                return true;
            }
            else return false;
        }
        public bool TryDropOn(TableField field)
        {
            if (Drawer == null)
                throw _ex;

            if (CanDropOn(field))
            {
                OnDropOn(field);
                return true;
            }
            else return false;
        }
        public Tween TryPullOut(bool sleevePull)
        {
            if (Drawer == null)
                throw _ex;
            Drawer.SetSortingAsTop();
            if (!sleevePull && Sleeve.Drawer.IsPulledOut)
                return null;
            if (!sleevePull && !CanPullOut())
                return null;
            return OnPullOut(sleevePull);
        }
        public Tween TryPullIn(bool sleevePull)
        {
            if (Drawer == null)
                throw _ex;
            Drawer.SetSortingAsDefault();
            if (!sleevePull && Sleeve.Drawer.IsPulledOut)
                return null;
            if (!sleevePull && !CanPullIn())
                return null;
            return OnPullIn(sleevePull);
        }

        public bool CanTake();
        public bool CanReturn();
        public bool CanDropOn(TableField field);
        public bool CanPullOut();
        public bool CanPullIn();

        public void OnTake();
        public void OnReturn();
        public void OnDropOn(TableField field);
        public Tween OnPullOut(bool sleevePull);
        public Tween OnPullIn(bool sleevePull);

        // --- use only in implementing class ---
        public void OnTakeBase()
        {
            Drawer.SetCollider(false);
            Sleeve.Remove(this);
            Sleeve.Drawer.CanPullOut = false;

            Drawer.transform.SetParent(Pointer.Transform);
            Drawer.SetSortingAsTop();
            Drawer.transform.DOKill();
        }
        public void OnReturnBase()
        {
            Drawer.SetCollider(true);

            Sleeve.Add(this);
            Sleeve.Drawer.CanPullOut = true;
        }
        public void OnDropOnBase(TableField field)
        {
            Drawer.SetCollider(true);

            Sleeve.Remove(this);
            Sleeve.Drawer.CanPullOut = true;

            Drawer.transform.SetParent(Global.Root);
            Drawer.SetSortingAsDefault();
        }
        public Tween OnPullOutBase(bool sleevePull)
        {
            float endY = sleevePull ? -1.25f : -0.22f;
            if (Sleeve.isForMe) endY = -endY;
            return Drawer.transform.DOLocalMoveY(endY, PULL_DURATION).SetEase(Ease.OutQuad);
        }
        public Tween OnPullInBase(bool sleevePull)
        {
            return Drawer.transform.DOLocalMoveY(0, PULL_DURATION).SetEase(Ease.OutQuad);
        }
        // ---       
    }
}
