using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using GreenOne;
using MyBox;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню с выбором карты поля из нескольких вариантов на добавление в колоду игрока.
    /// </summary>
    public class CardChooseMenu : Menu
    {
        const string ID = "card_choose";
        const int MAX_CARDS = 5;

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        public bool CardsAreShown => _cardsAreShown;
        public override string LinkedMusicMixId => "peace";

        public int CardPoints => _cardPoints;
        public int CardsCount => _cardsCount;
        public int RerollsLeft => _rerollsLeft;
        public int FieldsChoicesLeft => _fieldsChoicesLeft;
        public int FloatsChoicesLeft => _floatsChoicesLeft;
        public int AllChoicesLeft => _fieldsChoicesLeft + _floatsChoicesLeft;

        readonly Transform _cardsParent;
        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _descTextMesh;
        readonly RerollButtonDrawer _rerollButton;
        readonly DeclineButtonDrawer _declineButton;
        readonly ArrowsAnim[] _arrows;

        int _cardPoints; // increase to show more powerful cards
        int _cardsCount;
        int _rerollsLeft;
        int _fieldsChoicesLeft;
        int _floatsChoicesLeft;
        CardToChoose[] _cards;
        bool _cardsAreShown;
        bool _cardsAreFields;
        bool _animInProgress;

        // one of the field cards in this menu
        class CardToChoose
        {
            const float ANIM_DURATION = 0.33f;
            const float CARD_DISTANCE_X = 1f;

            const float CARD_MIN_Y = -0.25f;
            const float CARD_MAX_Y = 0.00f;

            static readonly Vector3 minScale = Vector3.one;
            static readonly Vector3 maxScale = Vector3.one * 1.15f;

            readonly CardChooseMenu _menu;
            readonly TableCard _onTable;

            public TableCardDrawer Drawer => _onTable.Drawer;
            public Card Data => _onTable.Data;

            bool _chosen;
            Tween _scaleTween;
            Tween _posTween;
            Tween _alphaTween;

            public CardToChoose(CardChooseMenu menu, TableCard onTable, int index)
            {
                _menu = menu;
                _onTable = onTable;

                float xHalfOffset = -menu._cardsCount / 2f + 0.5f;
                TableCardDrawer drawer = _onTable.Drawer;

                drawer.transform.localPosition = new Vector3(index * CARD_DISTANCE_X + xHalfOffset, CARD_MIN_Y);
                drawer.ChangePointer = true;

                drawer.OnMouseEnter += OnCardMouseEnter;
                drawer.OnMouseLeave += OnCardMouseLeave;
                drawer.OnMouseClick += OnCardMouseClickLeft;

                drawer.ColliderEnabled = false;
                drawer.SortingOrder = 0;
                drawer.Alpha = 0;
            }

            public Tween AnimShow()
            {
                Sequence seq = DOTween.Sequence();
                seq.AppendCallback(() =>
                {
                    AnimPosUp();
                    AnimFadeIn();
                });
                seq.AppendInterval(ANIM_DURATION);
                seq.OnComplete(() => Drawer.ColliderEnabled = _menu.ColliderEnabled);

                return seq.Play();
            }
            public Tween AnimHide()
            {
                Drawer.ColliderEnabled = false;
                Sequence seq = DOTween.Sequence();
                seq.AppendCallback(() =>
                {
                    AnimPosDown();
                    AnimFadeOut();
                });
                seq.AppendInterval(ANIM_DURATION);
                seq.OnComplete(_onTable.Dispose);

                return seq.Play();
            }

            Tween AnimScaleUp()
            {
                if (_chosen) return _scaleTween;
                _scaleTween.Kill();
                _scaleTween = Drawer.transform.DOScale(maxScale, ANIM_DURATION).SetEase(Ease.OutCubic);
                return _scaleTween;
            }
            Tween AnimScaleDown()
            {
                if (_chosen) return _scaleTween;
                _scaleTween.Kill();
                _scaleTween = Drawer.transform.DOScale(minScale, ANIM_DURATION).SetEase(Ease.OutCubic);
                return _scaleTween;
            }

            Tween AnimPosUp()
            {
                _posTween.Kill();
                _posTween = Drawer.transform.DOLocalMoveY(CARD_MAX_Y, ANIM_DURATION).SetEase(Ease.OutQuad);
                return _posTween;
            }
            Tween AnimPosDown()
            {
                _posTween.Kill();
                _posTween = Drawer.transform.DOLocalMoveY(CARD_MIN_Y, ANIM_DURATION).SetEase(Ease.InQuad);
                return _posTween;
            }

            Tween AnimFadeIn()
            {
                _alphaTween.Kill();
                _alphaTween = Drawer.DOFade(1, ANIM_DURATION);
                return _alphaTween;
            }
            Tween AnimFadeOut()
            {
                _alphaTween.Kill();
                _alphaTween = Drawer.DOFade(0, ANIM_DURATION);
                return _alphaTween;
            }

            void OnCardMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                AnimScaleUp();
                string desc = _onTable.DescDynamicWithLinks(out string[] links);
                Tooltip.SetAlign(HorizontalAlignmentOptions.Left);
                Tooltip.SetText(links.Prepend(desc).ToArray());
            }
            void OnCardMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                AnimScaleDown();
                Tooltip.ClearText();
            }
            void OnCardMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                _chosen = true;
                _ = _menu.ConfirmChoice(_onTable);
            }
        }
        class ArrowsAnim
        {
            readonly Transform _transform;
            readonly Tween _tween;

            public ArrowsAnim(Transform transform, float duration, float startPos, float endPos)
            {
                _transform = transform;
                _tween = DOVirtual.Float(startPos, endPos, duration, Update).SetTarget(_transform).SetLoops(-1);
                _tween.Pause();
            }

            public void Play()
            {
                _tween.Play();
            }
            public void Kill()
            {
                _tween.Kill();
            }

            void Update(float x)
            {
                _transform.localPosition = _transform.localPosition.SetX(x);
            }
        }

        class RerollButtonDrawer : Drawer
        {
            readonly CardChooseMenu _menu;
            readonly SpriteRenderer _renderer;
            readonly ColorPaletteSpriteElement _paletteElement;

            public RerollButtonDrawer(CardChooseMenu menu) : base(menu, menu.Transform.Find("Reroll button"))
            {
                _menu = menu;
                _renderer = gameObject.GetComponent<SpriteRenderer>();
                _paletteElement = gameObject.GetComponent<ColorPaletteSpriteElement>();
                _paletteElement.setColorOnStart = false;
                SetTooltip(() => $"Перебросить карты ({_menu._rerollsLeft}x). Обновите ассортимент карт, если текущий выбор вас не устраивает.");
            }

            protected override void SetColor(Color value)
            {
                base.SetColor(value);
                _renderer.color = value;
            }
            protected override void SetCollider(bool value)
            {
                base.SetCollider(value);
                SetColor(value ? _paletteElement.SyncedColor : Color.gray);
            }
            protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseClickBase(sender, e);
                if (!e.isLmbDown) return;
                _menu.RerollChoice();
            }
        }
        class DeclineButtonDrawer : Drawer
        {
            readonly CardChooseMenu _menu;
            readonly SpriteRenderer _renderer;
            readonly ColorPaletteSpriteElement _paletteElement;

            public DeclineButtonDrawer(CardChooseMenu menu) : base(menu, menu.Transform.Find("Decline button")) 
            {
                _menu = menu;
                _renderer = transform.GetComponent<SpriteRenderer>();
                _paletteElement = gameObject.GetComponent<ColorPaletteSpriteElement>();
                _paletteElement.setColorOnStart = false;
                SetTooltip(() => "<color=red>Отказаться от карты.</color> Полезно, когда выбирать из того, что есть, не хочется.");
            }
            public void SetColor()
            {
                SetColor(ColliderEnabled ? _paletteElement.SyncedColor : Color.gray);
            }

            protected override void SetColor(Color value)
            {
                base.SetColor(value);
                _renderer.color = value;
            }
            protected override void SetCollider(bool value)
            {
                base.SetCollider(value);
                SetColor();
            }

            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseEnterBase(sender, e);
                SetColor(Color.red);
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseLeaveBase(sender, e);
                SetColor();
            }
            protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseClickBase(sender, e);
                if (!e.isLmbDown) return;
                _menu.DeclineChoice();
            }
        }

        public CardChooseMenu(int cardPoints, int fieldsChoices, int floatsChoices, int cardsCount, int rerolls) : base(ID, _prefab)
        {
            _cardPoints = cardPoints;
            _cardsCount = cardsCount;
            _rerollsLeft = rerolls;
            _fieldsChoicesLeft = fieldsChoices;
            _floatsChoicesLeft = floatsChoices;

            _cards = Array.Empty<CardToChoose>();
            _cardsParent = Transform.Find("Cards");
            _headerTextMesh = Transform.Find<TextMeshPro>("Header text");
            _descTextMesh = Transform.Find<TextMeshPro>("Desc text");
            _rerollButton = new RerollButtonDrawer(this);
            _declineButton = new DeclineButtonDrawer(this);
            _arrows = new ArrowsAnim[]
            {
                new(Transform.Find("Arrows 1"), 50, -3.6f, 3.68f),
                new(Transform.Find("Arrows 2"), 30, 3.6f, -3.68f),
            };
        }

        public override void OnTransitStart(bool isFromThis)
        {
            if (!isFromThis)
                _animInProgress = true;
            base.OnTransitStart(isFromThis);
        }
        public override async void OnTransitEnd(bool isFromThis)
        {
            if (!isFromThis)
                await ShowCards();
            base.OnTransitEnd(isFromThis);
            if (!isFromThis)
                _animInProgress = false;
        }

        protected override void Open()
        {
            base.Open();
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Play();
        }
        protected override void Close()
        {
            base.Close();
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Kill();
        }
        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            foreach (CardToChoose card in _cards)
            {
                if (card.Drawer != null)
                    card.Drawer.ColliderEnabled = value;
            }
            _rerollButton.ColliderEnabled = value && _rerollsLeft > 0;
            _declineButton.ColliderEnabled = value;
        }

        Card[] GenerateCardsToChoose()
        {
            if (_cardsCount > MAX_CARDS)
                throw new Exception($"Cards count must not be greater than {MAX_CARDS}.");
            if (_cardsCount <= 0)
                throw new Exception("Cards count (amount of cards to choose from) was less or equal to 0.");

            if (_fieldsChoicesLeft > 0)
            {
                FieldCard[] genCards = new FieldCard[_cardsCount];
                for (int i = 0; i < _cardsCount; i++)
                {
                    string srcCardId = CardBrowser.GetFieldRandom().id;
                    FieldCard genCard = CardBrowser.NewField(srcCardId).ShuffleMainStats().UpgradeWithTraitAdd(_cardPoints);
                    genCards[i] = genCard;
                }
                return genCards;
            }
            else if (_floatsChoicesLeft > 0)
            {
                FloatCard[] genCards = new FloatCard[_cardsCount];
                for (int i = 0; i < _cardsCount; i++)
                {
                    string srcCardId = CardBrowser.GetFloatRandom().id;
                    FloatCard genCard = CardBrowser.NewFloat(srcCardId);
                    genCards[i] = genCard;
                }
                return genCards;
            }
            Debug.LogError("There are no card choices left to generate cards for.");
            return null;
        }

        async UniTask ConfirmChoice(TableCard? chosenCard)
        {
            if (AllChoicesLeft <= 0 || _animInProgress)
                return;
            if (chosenCard != null && Player.Deck.LimitReached)
            {
                chosenCard.Drawer.CreateTextAsSpeech("ПРЕДЕЛ КОЛОДЫ", Color.red);
                return;
            }

            _animInProgress = true;
            SetCollider(false);
            if (chosenCard != null)
            {
                Tween tween = chosenCard.Drawer.AnimHighlightOutline(1.5f);
                Card data = chosenCard.Data;
                if (data.isField)
                     Player.Deck.fieldCards.Add((FieldCard)data, true);
                else Player.Deck.floatCards.Add((FloatCard)data, true);
                await tween.AsyncWaitForCompletion();
            }

            if (_cardsAreFields)
                 _fieldsChoicesLeft--;
            else _floatsChoicesLeft--;
            
            await HideCards();
            if (AllChoicesLeft <= 0)
            {
                await TransitFromThis();
                return;
            }

            await ShowCards();
            _animInProgress = false;
            SetCollider(true);
        }
        async UniTask RerollChoice()
        {
            if (_rerollsLeft <= 0 || _animInProgress)
                return;

            _animInProgress = true;
            _rerollsLeft--;
            SetCollider(false);
            await HideCards();
            await ShowCards();
            _animInProgress = false;
            SetCollider(true);
        }
        async UniTask DeclineChoice() => await ConfirmChoice(null);

        public async UniTask ShowCards()
        {
            if (_cards.Length != 0)
                await HideCards();

            if (_cardsAreShown) return;
            _cardsAreShown = true;

            Tween lastTween = null;
            Card[] generatedCards = GenerateCardsToChoose();
            if (generatedCards == null) return;
            _cards = new CardToChoose[generatedCards.Length];
            _cardsAreFields = generatedCards[0].isField;

            for (int i = 0; i < generatedCards.Length; i++)
            {
                Card genCard = generatedCards[i];
                TableCard onTable = genCard.isField ? new TableFieldCard((FieldCard)genCard, _cardsParent) 
                                                    : new TableFloatCard((FloatCard)genCard, _cardsParent);
                CardToChoose cardToChoose = new(this, onTable, i);
                _cards[i] = cardToChoose;
                lastTween = cardToChoose.AnimShow();
            }

            await lastTween.AsyncWaitForCompletion();
        }
        public async UniTask HideCards()
        {
            if (_cards == null)
                throw new InvalidOperationException("There are no cards to hide.");

            if (!_cardsAreShown) return;
            _cardsAreShown = false;

            Tween lastTween = null;
            foreach (CardToChoose card in _cards)
                lastTween = card.AnimHide();

            _cards = Array.Empty<CardToChoose>();
            await lastTween.AsyncWaitForCompletion();
        }
    }
}
