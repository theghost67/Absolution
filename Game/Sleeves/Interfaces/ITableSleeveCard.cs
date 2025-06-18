using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Sleeves
{
    /// <summary>
    /// Реализует карту стола как карту рукава, добавляя возможность подбирать, возвращать и бросать карту на поле из рукава.<br/>
    /// Автоматически реализует механизм подбора таких карт игроком, где игрок сам решает, когда возвращать/бросать/возвращать карту.
    /// </summary>
    public interface ITableSleeveCard : ITableCard
    {
        public const float PULL_DURATION = 0.33f;
        public static bool IsHoldingAnyCard => _isHoldingAnyCard;
        public TableSleeve Sleeve { get; }

        private static bool _isHoldingAnyCard;
        private static bool _isInCooldown;
        private static ITableSleeveCard _card;
        private static readonly System.Exception _ex = new($"{nameof(ITableSleeveCard)} methods (except for {nameof(TryDropOn)}) should be invoked only by player (user) interaction and when the card has it's own {nameof(Drawer)} ({nameof(Sleeve)} must have drawer too).");

        static ITableSleeveCard() { Global.OnUpdate += OnUpdate; }
        public static void TryTakeCard(ITableSleeveCard card)
        {
            if (_isInCooldown || _isHoldingAnyCard || !card.TryTake())
                return;

            _card = card;
            _isHoldingAnyCard = true;

            _isInCooldown = true;
            DOVirtual.DelayedCall(0.12f, () => _isInCooldown = false);
        }

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
        public bool TryDropOn(TableSleeveCardDropArgs e)
        {
            if (CanDropOn(e.field))
            {
                OnDropOn(e);
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
        public void OnDropOn(TableSleeveCardDropArgs e);
        public Tween OnPullOut(bool sleevePull);
        public Tween OnPullIn(bool sleevePull);

        // --- use only in implementing class ---
        public void OnTakeBase()
        {
            Sleeve.Drawer.CanPullOut = false;
            Sleeve.Remove(this);

            Drawer.ColliderEnabled = false;
            Drawer.transform.SetParent(Pointer.Transform);
            Drawer.SetSortingAsTop();
            Drawer.transform.DOKill();
        }
        public void OnReturnBase()
        {
            Sleeve.Drawer.CanPullOut = true;
            Sleeve.Add(this);
        }
        public void OnDropOnBase(TableSleeveCardDropArgs e)
        {
            Sleeve.Drawer.CanPullOut = true;
            if (e.isPreview) return;
            if (Drawer.RendererIsFlipped.y)
                Drawer.FlipRendererY();
            Sleeve.Remove(this);
            Drawer.ColliderEnabled = true;
            Drawer.SortingOrderDefault = 10 + (e.field?.pos.x * 4 ?? 0);
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

        private static void TryReturnCard()
        {
            if (_isInCooldown || !_isHoldingAnyCard || !_card.TryReturn())
                return;

            _card = null;
            _isHoldingAnyCard = false;
        }
        private static void TryDropCard()
        {
            TableFieldDrawer drawer = (TableFieldDrawer)Game.Drawer.SelectedDrawers.FirstOrDefault(d => d is TableFieldDrawer);
            TableField field = drawer?.attached;

            if (field == null || _isInCooldown || !_isHoldingAnyCard || !_card.CanDropOn(field))
                return;

            ITableSleeveCard card = _card;
            _card = null;
            _isHoldingAnyCard = false;

            card.Drawer.transform.SetParent(Global.Root, true);
            card.TryDropOn(new TableSleeveCardDropArgs(field, true));
            PlayerQueue.Enqueue(new PlayerAction()
            {
                conditionFunc = () => card.CanTake() && card.CanDropOn(field),
                successFunc = () => card.TryDropOn(new TableSleeveCardDropArgs(field, false)),
                failFunc = () =>
                {
                    card.Drawer.CreateTextAsSpeech(Translator.GetString("i_table_sleeve_card_1"), Color.red);
                    card.Sleeve.Add(card);
                },
                abortFunc = () => card.Sleeve.Add(card),
                msDelay = 500,
            });
        }
        private static void OnUpdate()
        {
            if (!_isHoldingAnyCard) return;
            if (Input.GetMouseButtonDown(0))
                TryDropCard();
            else if (Input.GetMouseButtonDown(1))
                TryReturnCard();
        }
    }
}
