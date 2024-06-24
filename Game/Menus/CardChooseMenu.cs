using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Palette;
using GreenOne;
using MyBox;
using System;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню с выбором карты поля из нескольких вариантов на добавление в колоду игрока.
    /// </summary>
    public class CardChooseMenu : Menu // supports only field cards choose
    {
        const string ID = "card_choose";
        const int MAX_CARDS = 5;

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        public bool CardsAreShown => _cardsAreShown;

        public int CardPoints => _cardPoints;
        public int CardsCount => _cardsCount;
        public int RerollsLeft => _rerollsLeft;
        public int FieldsChoicesLeft => _fieldsChoicesLeft;
        public int FloatsChoicesLeft => _floatsChoicesLeft;

        readonly Transform _cardsParent;
        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _descTextMesh;
        readonly ArrowsAnim[] _arrows;

        int _cardPoints; // increase to show more powerful cards
        int _cardsCount;
        int _rerollsLeft; // TODO: imeplement
        int _fieldsChoicesLeft; // TODO: implement graphics?
        int _floatsChoicesLeft;
        CardToChoose[] _cards;
        bool _cardsAreShown;

        // one of the field cards in this menu
        class CardToChoose
        {
            const float ANIM_DURATION = 0.50f;
            const float CARD_DISTANCE_X = 1f;

            const float CARD_MIN_Y = -0.25f;
            const float CARD_MAX_Y = 0.00f;

            static readonly Vector3 minScale = Vector3.one;
            static readonly Vector3 maxScale = Vector3.one * 1.15f;

            public readonly CardChooseMenu menu;
            public readonly int index;

            readonly bool _isField;
            readonly TableCard _onTable;

            TableCardDrawer Drawer => _onTable.Drawer;
            Card Data => _onTable.Data;

            bool _chosen;
            Tween _scaleTween;
            Tween _posTween;
            Tween _alphaTween;

            public CardToChoose(CardChooseMenu menu, TableCard onTable, int index)
            {
                this.menu = menu;
                this.index = index;

                _isField = onTable.Data.isField;
                _onTable = onTable;
                _scaleTween = Utils.emptyTween;
                _posTween = Utils.emptyTween;
                _alphaTween = Utils.emptyTween;

                float xHalfOffset = -menu._cardsCount / 2f + 0.5f;
                TableCardDrawer drawer = _onTable.Drawer;

                drawer.transform.localPosition = new Vector3(index * CARD_DISTANCE_X + xHalfOffset, CARD_MIN_Y);
                drawer.ChangePointer = true;

                drawer.OnMouseEnter += OnCardMouseEnter;
                drawer.OnMouseLeave += OnCardMouseLeave;
                drawer.OnMouseClickLeft += OnCardMouseClickLeft;

                drawer.SetCollider(false);
                drawer.SetSortingOrder(0);
                drawer.SetAlpha(0);
            }

            public Tween AnimShow()
            {
                Sequence seq = DOTween.Sequence(Drawer);
                seq.AppendCallback(() =>
                {
                    AnimPosUp();
                    AnimFadeIn();
                });
                seq.AppendInterval(ANIM_DURATION * 2);
                seq.OnComplete(() => Drawer.SetCollider(true));

                return seq.Play();
            }
            public Tween AnimHide()
            {
                Sequence seq = DOTween.Sequence(Drawer);
                seq.AppendCallback(() =>
                {
                    AnimPosDown();
                    AnimFadeOut();
                });
                seq.AppendInterval(ANIM_DURATION * 2);

                return seq.Play();
            }

            Tween AnimScaleUp()
            {
                if (_chosen) return _scaleTween;
                _scaleTween.Kill();
                _scaleTween = Drawer.transform.DOScale(maxScale, ANIM_DURATION / 2).SetEase(Ease.OutCubic);
                return _scaleTween;
            }
            Tween AnimScaleDown()
            {
                if (_chosen) return _scaleTween;
                _scaleTween.Kill();
                _scaleTween = Drawer.transform.DOScale(minScale, ANIM_DURATION / 2).SetEase(Ease.OutCubic);
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
            }
            void OnCardMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                AnimScaleDown();
            }
            void OnCardMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                ConfirmChoice();
            }

            async void ConfirmChoice()
            {
                foreach (CardToChoose card in menu._cards)
                    card.Drawer.SetCollider(false);

                _chosen = true;
                Drawer.HighlightOutline(GetHighlightColor());

                if (_isField)
                {
                    Player.Deck.fieldCards.Add((FieldCard)Data);
                    menu._fieldsChoicesLeft--;
                }
                else
                {
                    Player.Deck.floatCards.Add((FloatCard)Data);
                    menu._floatsChoicesLeft--;
                }

                await UniTask.Delay(1500);
                await menu.HideCards();

                if (menu._fieldsChoicesLeft > 0 || menu._floatsChoicesLeft > 0)
                     await menu.ShowCards();
                else await menu.CloseAnimated();
            }
            Color GetHighlightColor()
            {
                if (_isField)
                {
                    Color color = Drawer.Outline.GetColor();
                    if (color.a != 0) return color;
                }
                return ColorPalette.GetColor(1);
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

        public CardChooseMenu(int cardPoints, int fieldsChoices, int floatsChoices = 0, int cardsCount = 3, int rerolls = 0) : base(ID, _prefab)
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
            _arrows = new ArrowsAnim[]
            {
                new(Transform.Find("Arrows 1"), 50, -3.6f, 3.68f),
                new(Transform.Find("Arrows 2"), 30, 3.6f, -3.68f),
            };
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Play();
            DOVirtual.DelayedCall(1, () => ShowCards());
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Kill();
        }

        public override void WriteDesc(string text)
        {
            base.WriteDesc(text);
            _descTextMesh.text = text;
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
                    FieldCard srcCard = CardBrowser.Fields.GetWeightedRandom(c => c.frequency);
                    FieldCard genCard = CardBrowser.NewField(srcCard.id).UpgradeWithTraitAdd(_cardPoints);
                    genCards[i] = genCard;
                }
                return genCards;
            }
            else if (_floatsChoicesLeft > 0)
            {
                FloatCard[] genCards = new FloatCard[_cardsCount];
                for (int i = 0; i < _cardsCount; i++)
                {
                    FloatCard srcCard = CardBrowser.Floats.GetWeightedRandom(c => c.frequency);
                    FloatCard genCard = CardBrowser.NewFloat(srcCard.id);
                    genCards[i] = genCard;
                }
                return genCards;
            }
            else throw new Exception("There are no card choices left to generate cards for.");
        }

        public async UniTask ShowCards()
        {
            if (_cards.Length != 0)
                await HideCards();

            if (_cardsAreShown) return;
            _cardsAreShown = true;

            Tween lastTween = Utils.emptyTween;
            Card[] generatedCards = GenerateCardsToChoose();
            _cards = new CardToChoose[generatedCards.Length];

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

            Tween lastTween = Utils.emptyTween;
            foreach (CardToChoose card in _cards)
                lastTween = card.AnimHide();

            _cards = Array.Empty<CardToChoose>();
            await lastTween.AsyncWaitForCompletion();
        }
    }
}
