using DG.Tweening;
using Game.Cards;
using Game.Territories;
using GreenOne;

namespace Game.Sleeves
{
    // TODO: move some implementation to ITableSleeveCardDrawer?
    /// <summary>
    /// Реализует карту стола как карту рукава, добавляя возможность подбирать, возвращать и бросать карту на поле из рукава.<br/>
    /// Примечание: карта должна иметь отрисовщик, а также, методы этого интерфейса должны вызываться только от лица пользователя (не от противника). 
    /// </summary>
    public interface ITableSleeveCard : ITableCard
    {
        public const float PULL_DURATION = 0.33f;
        public TableSleeve Sleeve { get; }

        // should be set only in ITableSleeveCard
        public bool IsInMove { get; protected set; }
        public bool IsPulledOut { get; protected set; }

        static readonly System.Exception _ex = new($"{nameof(ITableSleeveCard)} methods should be invoked only by player (user) interaction and when the card has it's own {nameof(Drawer)} ({nameof(Sleeve)} must have drawer too).");

        public bool TryTake()
        {
            if (Drawer == null)
                throw _ex;

            if (CanTake())
            {
                Take();
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
                Return();
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
                DropOn(field);
                return true;
            }
            else return false;
        }

        public bool TryPullOut()
        {
            if (Drawer == null)
                throw _ex;

            if (Sleeve.Drawer.IsInMove) return false;
            if (Sleeve.Drawer.IsPulledOut)
            {
                Drawer.SetSortingAsTop();
                return true;
            }

            if (CanPullOut())
            {
                PullOutBase();
                return true;
            }
            return false;
        }
        public bool TryPullIn()
        {
            if (Drawer == null)
                throw _ex;

            if (Sleeve.Drawer.IsInMove) return false;
            if (Sleeve.Drawer.IsPulledOut)
            {
                Drawer.SetSortingAsDefault();
                return true;
            }

            if (CanPullIn())
            {
                PullInBase();
                return true;
            }
            return false;
        }

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
        public void DropOnBase()
        {
            Drawer.SetCollider(true);

            Sleeve.Remove(this);
            Sleeve.Drawer.CanPullOut = true;

            Drawer.transform.SetParent(Global.Root);
            Drawer.SetSortingAsDefault();
        }
        // ---       

        protected abstract bool CanTake();
        protected abstract bool CanReturn();
        protected abstract bool CanDropOn(TableField field);

        protected abstract bool CanPullOut();
        protected abstract bool CanPullIn();

        protected abstract void Take();
        protected abstract void Return();
        protected abstract void DropOn(TableField field);

        void PullOutBase()
        {
            float endY = -0.22f.InversedIf(Sleeve.isForMe);
            IsInMove = true;
            Drawer.transform.DOLocalMoveY(endY, PULL_DURATION).SetEase(Ease.OutQuad).OnComplete(OnPulledOut);
        }
        void PullInBase()
        {
            const float END_Y = 0;
            IsInMove = true;
            Drawer.transform.DOLocalMoveY(END_Y, PULL_DURATION).SetEase(Ease.OutQuad).OnComplete(OnPulledIn);
        }

        void OnPulledOut()
        {
            IsInMove = false;
            IsPulledOut = true;
        }
        void OnPulledIn()
        {
            IsInMove = false;
            IsPulledOut = false;
        }
    }
}
