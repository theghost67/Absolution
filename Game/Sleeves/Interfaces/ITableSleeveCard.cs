using DG.Tweening;
using Game.Cards;
using Game.Territories;
using GreenOne;

namespace Game.Sleeves
{
    // TODO: recreate as ITableSleeveCardDrawer?
    /// <summary>
    /// Реализует карту стола как карту рукава, добавляя возможность подбирать, возвращать и бросать карту на поле из рукава.<br/>
    /// Примечание: карта должна иметь отрисовщик, а также, методы этого интерфейса должны вызываться только от лица пользователя (не от противника). 
    /// </summary>
    public interface ITableSleeveCard : ITableCard
    {
        public const float PULL_DURATION = 0.33f;
        public TableSleeve Sleeve { get; }

        // should be set only in ITableSleeveCard (here)
        public bool IsInMove { get; protected set; }
        public bool IsPulledOut { get; protected set; }
        private static readonly System.Exception _ex = new($"{nameof(ITableSleeveCard)} methods should be invoked only by player (user) interaction and when the card has it's own {nameof(Drawer)} ({nameof(Sleeve)} must have drawer too).");

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
        public bool TryPullOut(bool sleevePull)
        {
            if (Drawer == null)
                throw _ex;
            Drawer.SetSortingAsTop();
            if (!sleevePull && Sleeve.Drawer.IsPulledOut)
                return false;
            if (sleevePull || CanPullOut())
            {
                OnPullOut(sleevePull);
                return true;
            }
            return false;
        }
        public bool TryPullIn(bool sleevePull)
        {
            if (Drawer == null)
                throw _ex;
            Drawer.SetSortingAsDefault();
            if (!sleevePull && Sleeve.Drawer.IsPulledOut)
                return false;
            if (sleevePull || CanPullIn())
            {
                OnPullIn(sleevePull);
                return true;
            }
            return false;
        }

        public bool CanTake();
        public bool CanReturn();
        public bool CanDropOn(TableField field);
        public bool CanPullOut();
        public bool CanPullIn();

        public void OnTake();
        public void OnReturn();
        public void OnDropOn(TableField field);
        public void OnPullOut(bool sleevePull);
        public void OnPullIn(bool sleevePull);

        // --- use only in implementing class ---
        public void TakeBase()
        {
            IsInMove = false;
            IsPulledOut = false;
            Drawer.SetCollider(false);

            Sleeve.Remove(this);
            Sleeve.Drawer.CanPullOut = false;

            Drawer.transform.SetParent(Pointer.Transform);
            Drawer.SetSortingAsTop();
            Drawer.transform.DOKill();
        }
        public void ReturnBase()
        {
            Drawer.SetCollider(true);

            Sleeve.Add(this);
            Sleeve.Drawer.CanPullOut = true;
        }
        public void DropOnBase(TableField field)
        {
            Drawer.SetCollider(true);

            Sleeve.Remove(this);
            Sleeve.Drawer.CanPullOut = true;

            Drawer.transform.SetParent(Global.Root);
            Drawer.SetSortingAsDefault();
        }
        public void PullOutBase(bool sleevePull)
        {
            float endY = sleevePull ? -1.25f : -0.22f;
            if (Sleeve.isForMe) endY = -endY;

            IsInMove = true;
            Drawer.transform.DOLocalMoveY(endY, PULL_DURATION).SetEase(Ease.OutQuad).OnComplete(OnPulledOut);
        }
        public void PullInBase(bool sleevePull)
        {
            IsInMove = true;
            Drawer.transform.DOLocalMoveY(0, PULL_DURATION).SetEase(Ease.OutQuad).OnComplete(OnPulledIn);
        }
        // ---       

        private void OnPulledOut()
        {
            IsInMove = false;
            IsPulledOut = true;
        }
        private void OnPulledIn()
        {
            IsInMove = false;
            IsPulledOut = false;
        }
    }
}
