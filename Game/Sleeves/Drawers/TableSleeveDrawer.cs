using DG.Tweening;
using Game.Cards;
using GreenOne;
using MyBox;
using System;
using System.Linq;
using UnityEngine;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableSleeve"/>.
    /// </summary>
    public class TableSleeveDrawer : Drawer
    {
        public const float MOVE_DURATION = 0.33f;
        const int SORT_ORDER_START_VALUE = 32;
        const int SORT_ORDER_PER_CARD = 8;

        public bool CanPullOut
        {
            get => _canPullOut;
            set
            {
                _canPullOut = value;
                if (!_canPullOut && _isPulledOut)
                    PullIn();

                SetCollider(_canPullOut);
            }
        }
        public bool IsPulledOut => _isPulledOut;
        public bool IsMovedOut => _isMovedOut;

        public readonly new TableSleeve attached;
        static readonly AlignSettings _alignSettings;

        Tween _posYTween;
        float _normalPosY;
        float _moveOutPosY;
        float _pullOutPosY;

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

            _canPullOut = true;
            _normalPosY  = sleeve.isForMe ? -2.16f : 2.16f;
            _moveOutPosY = sleeve.isForMe ? -2.40f : 2.40f;
            _pullOutPosY = sleeve.isForMe ? -1.10f : 1.10f;
            transform.localPosition = Vector3.up * _normalPosY;

            Global.OnUpdate += OnUpdate;
            SetCollider(true);
            AddCreatingCardDrawer_ForAll();
        }

        public void AddCardDrawer(ITableSleeveCard card)
        {
            TableCardDrawer drawer = card.Drawer;
            if (drawer == null || drawer.IsDestroying) return;

            bool colliderState = ColliderEnabled;
            if (drawer.ColliderEnabled != colliderState)
                drawer.SetCollider(colliderState);

            drawer.transform.SetParent(transform);
            UpdateCardsPosAndOrder();

            TableSleeveCardComponent component = drawer.gameObject.GetOrAddComponent<TableSleeveCardComponent>();
            if (!component.Enabled)
                component.Attach(drawer);
        }
        public void RemoveCardDrawer(ITableSleeveCard card)
        {
            TableCardDrawer drawer = card.Drawer;
            if (drawer == null || drawer.IsDestroying) return;

            drawer.transform.SetParent(transform.parent, worldPositionStays: true);
            UpdateCardsPosAndOrder();

            if (drawer.gameObject.TryGetComponent(out TableSleeveCardComponent component))
                component.Detatch();
        }

        // can be pulled by user (see Update() method)
        public void PullOut()
        {
            _isPulledOut = true;
            _isMovedOut = false;

            //_posYTween.Kill();
            //_posYTween = transform.DOLocalMoveY(_pullOutPosY, MOVE_DURATION).SetEase(Ease.OutQuad).OnComplete(OnPulledOut);
            gameObject.SetActive(true);
            foreach (ITableSleeveCard card in attached)
                card.OnPullOut(true);
        }
        public void PullIn()
        {
            _isPulledOut = false;
            _isMovedOut = false;

            //_posYTween.Kill();
            //_posYTween = transform.DOLocalMoveY(_normalPosY, MOVE_DURATION).SetEase(Ease.OutQuad).OnComplete(OnPulledIn);
            gameObject.SetActive(true);
            foreach (ITableSleeveCard card in attached)
                card.OnPullIn(true);
        }

        public void MoveIn()
        {
            _isMovedOut = false;
            _isPulledOut = false;

            _posYTween.Kill();
            _posYTween = transform.DOLocalMoveY(_normalPosY, MOVE_DURATION).SetEase(Ease.OutQuad).OnComplete(OnMovedIn);
            gameObject.SetActive(true);
        }
        public void MoveOut()
        {
            _isMovedOut = true;
            _isPulledOut = false;

            _posYTween.Kill();
            _posYTween = transform.DOLocalMoveY(_moveOutPosY, MOVE_DURATION).SetEase(Ease.OutQuad).OnComplete(OnMovedOut);
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

        public override void SetCollider(bool value)
        {
            base.SetCollider(value);
            foreach (ITableSleeveCard card in attached)
                card.Drawer.SetCollider(value);
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

        void UpdateCardsPosAndOrder()
        {
            if (IsDestroying) return;
            const int THRESHOLD = 3;
            const float DISTANCE = TableCardDrawer.WIDTH - TableCardDrawer.WIDTH * 0.25f;

            int cardsCount = attached.Count;
            float[] cardsY = new float[cardsCount].FillBy(i => attached[i].Drawer.transform.position.y);

            if (transform.childCount > THRESHOLD)
                _alignSettings.distance.x = cardsCount < 4 ? DISTANCE : DISTANCE * (1 - (0.03f * cardsCount));
            else _alignSettings.distance.x = DISTANCE;

            _alignSettings.ApplyTo(i => attached[i].Drawer.transform, cardsCount);
            for (int i = 0; i < cardsCount; i++) 
            {
                ITableSleeveCard card = attached[i];
                card.Drawer.SetSortingOrder(SORT_ORDER_START_VALUE + i * SORT_ORDER_PER_CARD, asDefault: true);
                if (!(card.IsInMove || card.IsPulledOut)) continue;
                Transform transform = card.Drawer.transform;
                transform.position = transform.position.SetY(cardsY[i]); // restores Y pos which can be modified by PullIn/PullOut animations
            }
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
                if (attached.Any(c => c.Drawer.IsSelected))
                    return;

                if (_isPulledOut)
                     PullIn();
                else PullOut();
            }
        }
    }
}
