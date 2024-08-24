using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using GreenOne;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableSleeve"/>.
    /// </summary>
    public class TableSleeveDrawer : Drawer
    {
        const int SORT_ORDER_START_VALUE = 100;
        const int SORT_ORDER_PER_CARD = 8;
        const float ANIM_DURATION = 0.33f;

        public bool CanPullOut
        {
            get => _canPullOut;
            set
            {
                _canPullOut = value;
                if (!_canPullOut && _isPulledOut)
                    PullIn();
            }
        }
        public bool IsPulledOut => _isPulledOut;
        public bool IsMovedOut => _isMovedOut;

        public readonly new TableSleeve attached;
        static readonly AlignSettings _alignSettings;

        readonly HashSet<int> _shownCardsGuids; // used to play special animation on add
        Tween _posYTween;
        float _normalPosY;
        float _moveOutPosY;
        bool _canPullOut;
        bool _isPulledOut;
        bool _isMovedOut;

        static TableSleeveDrawer()
        {
            _alignSettings = new(Vector2.zero, AlignAnchor.MiddleCenter, 0, false, 1);
        }
        public TableSleeveDrawer(TableSleeve sleeve, Transform parent) : base(sleeve, parent.CreateEmptyObject("Sleeve"))
        {
            attached = sleeve;

            _shownCardsGuids = new HashSet<int>();
            _normalPosY  = sleeve.isForMe ? -2.16f : 2.16f;
            _moveOutPosY = sleeve.isForMe ? -2.40f : 2.40f;
            _canPullOut = true;
            transform.localPosition = Vector3.up * _normalPosY;

            Global.OnUpdate += OnUpdate;
            SetCollider(true);
            AddCreatingCardDrawer_ForAll();
        }

        public void AddCardDrawer(ITableSleeveCard card)
        {
            TableCardDrawer drawer = card.Drawer;
            if (drawer == null || drawer.IsDestroying) return;

            drawer.ColliderEnabled = ColliderEnabled;
            drawer.transform.SetParent(transform);
            TableSleeveCardComponent component = drawer.gameObject.GetOrAddComponent<TableSleeveCardComponent>();
            if (!component.Enabled)
                component.Attach(drawer);

            if (!gameObject.activeInHierarchy)
            {
                UpdateCardsPosAndOrderInstantly();
                return;
            }

            UpdateCardsPosAndOrderAnimated();
            _shownCardsGuids.Add(card.Guid);
        }
        public void RemoveCardDrawer(ITableSleeveCard card)
        {
            TableCardDrawer drawer = card.Drawer;
            if (drawer == null || drawer.IsDestroying) return;

            if (!ColliderEnabled) drawer.ColliderEnabled = true;
            drawer.transform.SetParent(transform.parent, worldPositionStays: true);
            UpdateCardsPosAndOrderAnimated();
            _shownCardsGuids.Remove(card.Guid);

            if (drawer.gameObject.TryGetComponent(out TableSleeveCardComponent component))
                component.Detatch();
        }

        // can be pulled by user (see OnUpdate method)
        public virtual void PullOut()
        {
            if (_isPulledOut) return;
            _isPulledOut = true;
            _isMovedOut = false;

            gameObject.SetActive(true);
            foreach (ITableSleeveCard card in attached)
                card.OnPullOut(true);
        }
        public virtual void PullIn()
        {
            if (!_isPulledOut) return;
            _isPulledOut = false;
            _isMovedOut = false;

            gameObject.SetActive(true);
            foreach (ITableSleeveCard card in attached)
                card.OnPullIn(true);
        }

        public void MoveIn()
        {
            _isMovedOut = false;
            _isPulledOut = false;

            _posYTween.Kill();
            _posYTween = transform.DOLocalMoveY(_normalPosY, ITableSleeveCard.PULL_DURATION).SetEase(Ease.OutQuad).OnComplete(OnMovedIn);
            gameObject.SetActive(true);
        }
        public void MoveOut()
        {
            _isMovedOut = true;
            _isPulledOut = false;

            _posYTween.Kill();
            _posYTween = transform.DOLocalMoveY(_moveOutPosY, ITableSleeveCard.PULL_DURATION).SetEase(Ease.OutQuad).OnComplete(OnMovedOut);
        }

        public void MoveInInstantly()
        {
            _isMovedOut = false;
            _isPulledOut = false;

            _posYTween.Kill();
            transform.localPosition = transform.localPosition.SetY(_normalPosY);
            gameObject.SetActive(true);
        }
        public void MoveOutInstantly()
        {
            _isMovedOut = true;
            _isPulledOut = false;

            _posYTween.Kill();
            transform.localPosition = transform.localPosition.SetY(_moveOutPosY);
            gameObject.SetActive(false);
        }

        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            foreach (ITableSleeveCard card in attached)
                card.Drawer.ColliderEnabled = value;
        }
        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();
            Global.OnUpdate -= OnUpdate;
        }
        protected virtual bool UpdateUserInput() => _canPullOut;

        void AddCreatingCardDrawer_ForAll()
        {
            foreach (ITableSleeveCard card in attached)
            {
                if (card.Drawer == null)
                     AddCreatingCardDrawer(card);
                else AddCardDrawer(card);
            }
        }
        void AddCreatingCardDrawer(ITableSleeveCard card)
        {
            card.CreateDrawer(transform);
            AddCardDrawer(card);
        }
        void RemoveDestroyingCardDrawer(ITableSleeveCard card, bool instantly = false)
        {
            RemoveCardDrawer(card);
            card.DestroyDrawer(instantly);
        }

        UniTask UpdateCardsPosAndOrderAnimated()
        {
            if (IsDestroying) return UniTask.CompletedTask;
            const int THRESHOLD = 3;
            const float DISTANCE = TableCardDrawer.WIDTH - TableCardDrawer.WIDTH * 0.25f;

            int cardsCount = attached.Count;
            Vector3[] cardsPositions = attached.Select(c => c.Drawer.transform.localPosition).ToArray();
            Tween lastTween = null;

            if (transform.childCount > THRESHOLD)
                _alignSettings.distance.x = cardsCount < 4 ? DISTANCE : DISTANCE * (1 - (0.03f * cardsCount));
            else _alignSettings.distance.x = DISTANCE;

            DOTween.Kill(attached.Guid);
            _alignSettings.ApplyTo(attached.Select(c => c.Drawer.transform).ToArray());
            for (int i = 0; i < attached.Count; i++)
            {
                ITableSleeveCard card = attached[i];
                float newPosX = card.Drawer.transform.localPosition.x;
                card.Drawer.SortingOrderDefault = SORT_ORDER_START_VALUE + i * SORT_ORDER_PER_CARD;
                if (_shownCardsGuids.Contains(card.Guid))
                {
                    card.Drawer.transform.localPosition = cardsPositions[i];
                    lastTween = card.Drawer.transform.DOLocalMoveX(newPosX, ANIM_DURATION).SetEase(Ease.OutQuad).SetId(attached.Guid);
                }
                else // plays 'add' animation
                {
                    card.Drawer.transform.localPosition = new Vector3(newPosX, _moveOutPosY);
                    if (_isPulledOut)
                         lastTween = card.OnPullOut(true);
                    else lastTween = card.Drawer.transform.DOLocalMoveY(0, ANIM_DURATION).SetEase(Ease.OutQuad);
                }
            }
            return lastTween.AsyncWaitForCompletion();
        }
        void UpdateCardsPosAndOrderInstantly()
        {
            if (IsDestroying) return;
            const int THRESHOLD = 3;
            const float DISTANCE = TableCardDrawer.WIDTH - TableCardDrawer.WIDTH * 0.25f;

            int cardsCount = attached.Count;
            float[] cardsY = new float[cardsCount].FillBy(i => attached[i].Drawer.transform.position.y);

            if (transform.childCount > THRESHOLD)
                _alignSettings.distance.x = cardsCount < 4 ? DISTANCE : DISTANCE * (1 - (0.03f * cardsCount));
            else _alignSettings.distance.x = DISTANCE;

            DOTween.Kill(attached.Guid);
            _alignSettings.ApplyTo(i => attached[i].Drawer.transform, cardsCount);
            for (int i = 0; i < cardsCount; i++)
                attached[i].Drawer.SortingOrderDefault = SORT_ORDER_START_VALUE + i * SORT_ORDER_PER_CARD;
        }

        void OnMovedOut()
        {
            _isMovedOut = true;
            gameObject.SetActive(false);
        }
        void OnMovedIn()
        {
            _isMovedOut = false;
        }
        void OnUpdate()
        {
            if (!UpdateUserInput()) return;
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_isPulledOut)
                     PullIn();
                else PullOut();
            }
        }
    }
}
