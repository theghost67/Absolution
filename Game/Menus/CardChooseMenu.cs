using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
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

        public int cardStatPoints; // increase to show more powerful cards
        public int cardsCount;
        public int rerollsLeft; // TODO: imeplement
        public int choicesLeft; // TODO: implement (graphics)

        readonly Transform _cardsParent;
        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _descTextMesh;
        readonly Arrows[] _arrows;

        readonly Dictionary<string, CardToChoose> _cards;
        bool _cardsAreShown;

        // one of the field cards in this menu
        class CardToChoose : TableFieldCard
        {
            const float ANIM_DURATION = 0.50f;
            const float CARD_DISTANCE_X = 1f;

            const float CARD_MIN_Y = -0.25f;
            const float CARD_MAX_Y = 0.00f;

            static readonly Vector3 minScale = Vector3.one;
            static readonly Vector3 maxScale = Vector3.one * 1.15f;

            public readonly CardChooseMenu menu;
            public readonly int index;

            bool _chosen;
            Tween _scaleTween;
            Tween _posTween;
            Tween _alphaTween;

            public CardToChoose(CardChooseMenu menu, FieldCard data, int index) : base(data, menu._cardsParent)
            {
                this.menu = menu;
                this.index = index;

                _scaleTween = Utils.emptyTween;
                _posTween = Utils.emptyTween;
                _alphaTween = Utils.emptyTween;

                float xHalfOffset = -menu.cardsCount / 2f + 0.5f;
                Drawer.transform.localPosition = new Vector3(index * CARD_DISTANCE_X + xHalfOffset, CARD_MIN_Y);
                Drawer.ChangePointer = true;

                Drawer.OnMouseEnter += OnCardMouseEnter;
                Drawer.OnMouseLeave += OnCardMouseLeave;
                Drawer.OnMouseClickLeft += OnCardMouseClickLeft;

                Drawer.SetCollider(false);
                Drawer.SetSortingOrder(0);
                Drawer.SetAlpha(0);
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
                foreach (CardToChoose card in menu._cards.Values)
                    card.Drawer.SetCollider(false);

                _chosen = true;
                Player.Deck.fieldCards.Add(Data);
                Drawer.HighlightOutline(Drawer.Outline.GetColor());

                await UniTask.Delay(1500);
                await menu.HideCards();

                if (--menu.choicesLeft > 0)
                     await menu.ShowCards();
                else await menu.CloseAnimated();
            }
        }
        class Arrows
        {
            const float POS_X1 = -3.6f;
            const float POS_X2 = 3.68f;

            readonly Transform _transform;
            readonly bool _movesRight;
            readonly float _duration;
            Tween _tween;

            public Arrows(Transform transform, float animDuration, bool movesRight)
            {
                _transform = transform;
                _duration = animDuration;
                _movesRight = movesRight;
                _tween = Utils.emptyTween;
            }

            public void AnimStart()
            {
                float startPos = _movesRight ? POS_X1 : POS_X2;
                float endPos = _movesRight ? POS_X2 : POS_X1;
                _tween = DOVirtual.Float(startPos, endPos, _duration, Update).SetTarget(_transform).SetLoops(-1);
            }
            public void AnimKill()
            {
                _tween.Kill();
            }
            void Update(float x)
            {
                _transform.localPosition = _transform.localPosition.SetX(x);
            }
        }

        public CardChooseMenu() : base(ID, _prefab)
        {
            _cards = new Dictionary<string, CardToChoose>();

            _cardsParent = Transform.Find("Cards");
            _headerTextMesh = Transform.Find<TextMeshPro>("Header text");
            _descTextMesh = Transform.Find<TextMeshPro>("Desc text");
            _arrows = new Arrows[]
            {
                new(Transform.Find("Arrows 1"), 80, true),
                new(Transform.Find("Arrows 2"), 60, false),
            };
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            foreach (Arrows arrows in _arrows)
                arrows.AnimStart();
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            foreach (Arrows arrows in _arrows)
                arrows.AnimKill();
        }

        public override async UniTask OpenAnimated()
        {
            await base.OpenAnimated();
            await ShowCards();
        }
        public override void WriteDesc(string text)
        {
            base.WriteDesc(text);
            _descTextMesh.text = text;
        }

        FieldCard[] GenerateCardsToChoose()
        {
            if (cardsCount > MAX_CARDS)
                throw new Exception($"Max cards to choose from equals to {MAX_CARDS}.");
            if (cardsCount <= 0)
                throw new Exception("Cards count (amount of cards to choose from) was not set.");

            FieldCard[] genCards = new FieldCard[cardsCount];
            for (int i = 0; i < cardsCount; i++)
            {
                FieldCard srcCard = CardBrowser.Fields.GetWeightedRandom(c => c.frequency);
                FieldCard genCard = CardBrowser.NewField(srcCard.id).UpgradeAsNewWithTraitAdd(cardStatPoints);
                genCards[i] = genCard;
            }
            return genCards;
        }

        public async UniTask ShowCards()
        {
            if (_cards.Count != 0)
                await HideCards();

            if (_cardsAreShown) return;
            _cardsAreShown = true;

            Tween lastTween = Utils.emptyTween;
            FieldCard[] generatedCards = GenerateCardsToChoose();
            for (int i = 0; i < generatedCards.Length; i++)
            {
                FieldCard generatedCard = generatedCards[i];
                CardToChoose cardToChoose = new(this, generatedCard, i);
                _cards.Add(cardToChoose.GuidStr, cardToChoose);
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
            foreach (CardToChoose card in _cards.Values)
                lastTween = card.AnimHide();

            _cards.Clear();
            await lastTween.AsyncWaitForCompletion();
        }
    }
}
