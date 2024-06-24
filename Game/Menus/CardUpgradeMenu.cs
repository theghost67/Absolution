﻿using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Sleeves;
using Game.Territories;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Game.Menus
{
    // TODO: create separate class for buttons (and derived?)
    // TODO: display upgraded desc from the right side

    /// <summary>
    /// Класс, представляющий меню с возможностью улучшать карты поля колоды игрока.
    /// </summary>
    public class CardUpgradeMenu : Menu
    {
        const string ID = "card_upgrade";

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        public CardDeck Deck => _sleeve.Deck;

        public float PointsLimit => _pointsLimit;
        public float PointsCurrent => _pointsCurrent;
        public event Action<float> OnDeckPointsChanged;

        readonly InfoButtonDrawer _infoButton;
        readonly ResetAllButtonDrawer _resetAllButton;
        readonly ResetThisButtonDrawer _resetThisButton;
        readonly FinishButtonDrawer _finishButton;

        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _descLeftTextMesh;
        readonly TextMeshPro _descRightTextMesh;

        readonly TextMeshPro _infoLeftTextMesh;
        readonly TextMeshPro _infoRightTextMesh;
        readonly ArrowsAnim[] _arrows;

        readonly SleeveToUpgrade _sleeve;
        readonly float _pointsLimit;
        readonly float _pointsAvailableAtStart;
        float _pointsCurrent;
        float _pointsAvailable;

        // one of the field cards in this menu
        // note: all cards stay in the sleeve, even on selection
        class CardToUpgrade : TableFieldCard, ITableSleeveCard
        {
            const float ANIM_DURATION = 0.50f;
            public static CardToUpgrade Selected => _selected;
            public static event Action OnSelectedChanged;
            public static event Action<float> OnAnyStatGraded; // float = delta
            public static event Action<float> OnAnyCardReset; 

            public TableSleeve Sleeve => _sleeve;
            public ITableSleeveCard AsInSleeve => this;

            public Vector3 DefaultPos => _defaultPos;
            public float DefaultPoints => _defaultPoints;
            public float CurrentPoints => Data.Points();

            bool ITableSleeveCard.IsInMove { get => _isPulling; set => _isPulling = value; }
            bool ITableSleeveCard.IsPulledOut { get => _isPulledOut; set => _isPulledOut = value; }

            static readonly Vector3 minScale = Vector3.one;
            static readonly Vector3 maxScale = Vector3.one * 1.25f;

            static CardToUpgrade _selected;
            readonly CardUpgradeMenu _menu;
            readonly TableSleeve _sleeve;
            readonly StatToUpgrade[] _stats; 

            Color _lastOutlineColor;
            Tween _scaleTween;
            Tween _posTween;

            Vector3 _defaultPos;
            float _defaultPoints;

            bool _isPulling;
            bool _isPulledOut;

            public CardToUpgrade(CardUpgradeMenu menu, FieldCard data, TableSleeve sleeve) : base(data, sleeve.Drawer.transform) 
            { 
                _menu = menu;
                _sleeve = sleeve;

                _stats = new StatToUpgrade[4 + data.traits.Count]; // card main stats count in the game = 4
                _stats[0] = new PriceStatToUpgrade(menu, this, Drawer.upperLeftIcon);
                _stats[1] = new MoxieStatToUpgrade(menu, this, Drawer.upperRightIcon);
                _stats[2] = new HealthStatToUpgrade(menu, this, Drawer.lowerLeftIcon);
                _stats[3] = new StrengthStatToUpgrade(menu, this, Drawer.lowerRightIcon);
                int index = 4;
                foreach (Trait trait in data.traits.Select(e => e.Trait))
                    _stats[index++] = new TraitStatToUpgrade(menu, this, Drawer.Traits.elements[trait.id]);
                foreach (StatToUpgrade stat in _stats)
                    stat.OnGraded += delta => OnAnyStatGraded(delta);

                _scaleTween = Utils.emptyTween;
                _posTween = Utils.emptyTween;

                Drawer.ChangePointer = true;
                Drawer.OnMouseClickLeft += (s, e) => { if (_selected != this) Select(); };
            }
            public static void OnUpdate()
            {
                if (_selected != null && Input.GetKeyDown(KeyCode.Escape))
                    _selected.Deselect(false);
            }

            bool ITableSleeveCard.CanTake() => false;
            bool ITableSleeveCard.CanReturn() => false;
            bool ITableSleeveCard.CanDropOn(TableField field) => false;

            bool ITableSleeveCard.CanPullOut() => _selected != this && Drawer.ColliderEnabled;
            bool ITableSleeveCard.CanPullIn() => _selected != this && Drawer.ColliderEnabled;

            void ITableSleeveCard.Take() => AsInSleeve.TakeBase();
            void ITableSleeveCard.Return() => AsInSleeve.ReturnBase();
            void ITableSleeveCard.DropOn(TableField field) => AsInSleeve.DropOnBase();

            // set once sleeve drawer created
            public void SetDefaults(Vector3 pos, float points)
            {
                _defaultPos = pos;
                _defaultPoints = points;
            }
            public async UniTask TryReset()
            {
                if (!ResetAvailable()) return;
                float startPoints = CurrentPoints;

                foreach (StatToUpgrade stat in _stats)
                    await stat.Reset();

                float pointsDelta = CurrentPoints - startPoints;
                OnAnyCardReset?.Invoke(pointsDelta);
            }
            public bool ResetAvailable()
            {
                foreach (StatToUpgrade stat in _stats)
                {
                    if (stat.UpgradedTimes != 0)
                        return true;
                }
                return false;
            }

            public Tween AnimCardSelect()
            {
                Sequence seq = DOTween.Sequence(Drawer);
                seq.AppendCallback(() =>
                {
                    AnimScaleUp();
                    AnimPosUp();
                });
                seq.AppendInterval(ANIM_DURATION);
                seq.OnComplete(() => Drawer.SetCollider(true));

                return seq.Play();
            }
            public Tween AnimCardDeselect()
            {
                Sequence seq = DOTween.Sequence(Drawer);
                seq.AppendCallback(() =>
                {
                    AnimScaleDown();
                    AnimPosDown();
                });
                seq.AppendInterval(ANIM_DURATION);
                seq.OnComplete(() => Drawer.SetCollider(true));

                return seq.Play();
            }

            public void AnimHighlightAsUpgrade()
            {
                if (_lastOutlineColor.a != 0 && Drawer.Outline.GetColorTween().IsPlaying())
                    Drawer.Outline.SetColor(_lastOutlineColor);
                _lastOutlineColor = Drawer.Outline.GetColor();
                Drawer.HighlightOutline(_lastOutlineColor, 0.5f);
            }
            public void AnimHighlightAsDowngrade()
            {
                Drawer.HighlightOutline(new Color(1.0f, 0.2f, 0.2f), 0.5f);
            }

            Tween AnimScaleUp()
            {
                _scaleTween.Kill();
                _scaleTween = Drawer.transform.DOScale(maxScale, ANIM_DURATION).SetEase(Ease.InCubic);
                return _scaleTween;
            }
            Tween AnimScaleDown()
            {
                _scaleTween.Kill();
                _scaleTween = Drawer.transform.DOScale(minScale, ANIM_DURATION).SetEase(Ease.OutCubic);
                return _scaleTween;
            }

            Tween AnimPosUp()
            {
                _posTween.Kill();
                _posTween = Drawer.transform.DOMove(Vector3.zero, ANIM_DURATION).SetEase(Ease.OutQuad);
                return _posTween;
            }
            Tween AnimPosDown()
            {
                _posTween.Kill();
                _posTween = Drawer.transform.DOMove(_defaultPos, ANIM_DURATION).SetEase(Ease.InQuad);
                return _posTween;
            }

            async UniTask Select()
            {
                Sleeve.Drawer.CanPullOut = false;

                foreach (ITableSleeveCard sleeveCard in _sleeve)
                    sleeveCard.Drawer.SetCollider(false);

                _selected?.Deselect(true);
                _selected = this;
                OnSelectedChanged?.Invoke();

                await AnimCardSelect().AsyncWaitForCompletion();

                foreach (ITableSleeveCard sleeveCard in _sleeve)
                    sleeveCard.Drawer.SetCollider(true);

                Sleeve.Drawer.CanPullOut = true;
            }
            async UniTask Deselect(bool insideOfSelect)
            {
                _selected = null;
                if (!insideOfSelect)
                {
                    OnSelectedChanged?.Invoke();
                    Sleeve.Drawer.CanPullOut = false;
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.SetCollider(false);
                }

                await AnimCardDeselect().AsyncWaitForCompletion();

                if (!insideOfSelect)
                {
                    Sleeve.Drawer.CanPullOut = true;
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.SetCollider(true);
                }
            }
        }
        class SleeveToUpgrade : TableSleeve
        {
            readonly CardUpgradeMenu _menu;
            public SleeveToUpgrade(CardUpgradeMenu menu, CardDeck deck) : base(deck, true, menu.Transform) { _menu = menu; }
            protected override ITableSleeveCard HoldingCardCreator(Card data)
            {
                if (data.isField)
                     return new CardToUpgrade(_menu, (FieldCard)data, this);
                else return null;
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

            void Update(float y)
            {
                _transform.localPosition = _transform.localPosition.SetY(y);
            }
        }

        abstract class StatToUpgrade
        {
            public event Action<float> OnGraded;
            public int UpgradedTimes => upgradedTimes;
            public float UpgradePointsDelta => upgradePointsDelta;
            public float DowngradePointsDelta => downgradePointsDelta;

            public readonly CardToUpgrade card;
            protected readonly CardUpgradeMenu menu;
            protected readonly Drawer statDrawer;

            protected int upgradedTimes;
            protected float upgradePointsDelta;
            protected float downgradePointsDelta;

            public StatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer)
            {
                this.menu = menu;
                this.card = card;
                this.statDrawer = statDrawer;

                statDrawer.OnMouseEnter += OnMouseEnter;
                statDrawer.OnMouseLeave += OnMouseLeave;
                statDrawer.OnMouseClickLeft += OnMouseClickLeft;
                statDrawer.OnMouseClickRight += OnMouseClickRight;
            }
            public abstract UniTask Reset();

            protected abstract UniTask Upgrade();
            protected abstract UniTask Downgrade();

            protected virtual bool UpgradeIsPossible() => true;

            protected abstract void OnMouseEnter(object sender, DrawerMouseEventArgs e);
            protected abstract void OnMouseLeave(object sender, DrawerMouseEventArgs e);

            async void OnMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                if (menu._pointsAvailable - upgradePointsDelta < -9999) return;

                card.AnimHighlightAsUpgrade();
                statDrawer.IsSelected = false; // force mouse enter invoke

                menu.SetColliders(false);
                await Upgrade();
                menu.SetColliders(true);

                OnGraded?.Invoke(upgradePointsDelta);
            }
            async void OnMouseClickRight(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                if (menu._pointsAvailable - downgradePointsDelta < -9999) return;
                if (upgradedTimes <= 0) return;

                card.AnimHighlightAsDowngrade();
                statDrawer.IsSelected = false; // force mouse enter invoke

                menu.SetColliders(false);
                await Downgrade();
                menu.SetColliders(true);

                OnGraded?.Invoke(downgradePointsDelta);
            }
        }
        abstract class ButtonDrawer : Drawer
        {
            protected readonly TextMeshPro textMesh;
            protected readonly CardUpgradeMenu menu;

            protected ButtonDrawer(CardUpgradeMenu menu, GameObject worldObject) : this(menu, worldObject.transform) { }
            protected ButtonDrawer(CardUpgradeMenu menu, Transform worldTransform) : base(null, worldTransform)
            {
                this.menu = menu;
                textMesh = gameObject.GetComponent<TextMeshPro>();
            }
        }

        class PriceStatToUpgrade : StatToUpgrade
        {
            public PriceStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) { }
            public override UniTask Reset()
            {
                int times = upgradedTimes;
                if (times == 0) return UniTask.CompletedTask;
                card.Data.price.value -= times;
                upgradedTimes = 0;
                return card.price.SetValueAbs(card.Data.price.value);
            }

            protected override UniTask Upgrade()
            {
                card.Data.price.value++;
                upgradedTimes++;
                return card.price.SetValueAbs(card.Data.price.value);
            }
            protected override UniTask Downgrade()
            {
                card.Data.price.value--;
                upgradedTimes--;
                return card.price.SetValueAbs(card.Data.price.value);
            }

            protected override void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                upgradePointsDelta = card.Data.PointsDeltaForPrice(1);
                if (upgradedTimes == 0)
                    downgradePointsDelta = 0;
                else downgradePointsDelta = card.Data.PointsDeltaForPrice(-1);
                menu.UpgradeInfoShow(upgradePointsDelta, downgradePointsDelta);
            }
            protected override void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                menu.UpgradeInfoHide();
            }
        }
        class MoxieStatToUpgrade : StatToUpgrade
        {
            public MoxieStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) { }
            public override UniTask Reset()
            {
                int times = upgradedTimes;
                if (times == 0) return UniTask.CompletedTask;
                card.Data.moxie -= times;
                upgradedTimes = 0;
                return card.moxie.SetValueAbs(card.Data.moxie);
            }

            protected override UniTask Upgrade()
            {
                card.Data.moxie++;
                upgradedTimes++;
                return card.moxie.SetValueAbs(card.Data.moxie);
            }
            protected override UniTask Downgrade()
            {
                card.Data.moxie--;
                upgradedTimes--;
                return card.moxie.SetValueAbs(card.Data.moxie);
            }

            protected override void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                upgradePointsDelta = card.Data.PointsDeltaForMoxie(1);
                if (upgradedTimes == 0)
                     downgradePointsDelta = 0;
                else downgradePointsDelta = card.Data.PointsDeltaForMoxie(-1);
                menu.UpgradeInfoShow(upgradePointsDelta, downgradePointsDelta);
            }
            protected override void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                menu.UpgradeInfoHide();
            }
        }
        class HealthStatToUpgrade : StatToUpgrade
        {
            public HealthStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) { }
            public override UniTask Reset()
            {
                int times = upgradedTimes;
                if (times == 0) return UniTask.CompletedTask;
                card.Data.health -= times;
                upgradedTimes = 0;
                return card.health.SetValueAbs(card.Data.health);
            }

            protected override UniTask Upgrade()
            {
                card.Data.health++;
                upgradedTimes++;
                return card.health.SetValueAbs(card.Data.health);
            }
            protected override UniTask Downgrade()
            {
                card.Data.health--;
                upgradedTimes--;
                return card.health.SetValueAbs(card.Data.health);
            }

            protected override void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                upgradePointsDelta = card.Data.PointsDeltaForHealth(1);
                if (upgradedTimes == 0)
                    downgradePointsDelta = 0;
                else downgradePointsDelta = card.Data.PointsDeltaForHealth(-1);
                menu.UpgradeInfoShow(upgradePointsDelta, downgradePointsDelta);
            }
            protected override void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                menu.UpgradeInfoHide();
            }
        }
        class StrengthStatToUpgrade : StatToUpgrade
        {
            public StrengthStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) {  }
            public override UniTask Reset()
            {
                int times = upgradedTimes;
                if (times == 0) return UniTask.CompletedTask;
                card.Data.strength -= times;
                upgradedTimes = 0;
                return card.strength.SetValueAbs(card.Data.strength);
            }

            protected override UniTask Upgrade()
            {
                card.Data.strength++;
                upgradedTimes++;
                return card.strength.SetValueAbs(card.Data.strength);
            }
            protected override UniTask Downgrade()
            {
                card.Data.strength--;
                upgradedTimes--;
                return card.strength.SetValueAbs(card.Data.strength);
            }

            protected override void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                upgradePointsDelta = card.Data.PointsDeltaForStrength(1);
                if (upgradedTimes == 0)
                    downgradePointsDelta = 0;
                else downgradePointsDelta = card.Data.PointsDeltaForStrength(-1);
                menu.UpgradeInfoShow(upgradePointsDelta, downgradePointsDelta);
            }
            protected override void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                menu.UpgradeInfoHide();
            }
        }
        class TraitStatToUpgrade : StatToUpgrade
        {
            public TraitStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, TableTraitListElementDrawer statDrawer) : base(menu, card, statDrawer)
            {
                statDrawer.enqueueAnims = false;
            }
            public override UniTask Reset()
            {
                int times = UpgradedTimes;
                if (times == 0) return UniTask.CompletedTask;

                TableTraitListElement element = (TableTraitListElement)statDrawer.attached;
                Trait trait = element.Trait.Data;
                int stacks = card.Data.traits[trait.id].Stacks - times;

                card.Data.traits.SetStacks(trait, stacks);
                upgradedTimes = 0;

                element.Drawer.AnimAdjust(stacks);
                return element.List.SetStacks(trait.id, stacks, null, null);
            }

            protected override UniTask Upgrade()
            {
                TableTraitListElement element = (TableTraitListElement)statDrawer.attached;
                Trait trait = element.Trait.Data;
                int stacks = card.Data.traits[trait.id].Stacks + 1;

                card.Data.traits.SetStacks(trait, stacks);
                upgradedTimes++;
                return element.List.SetStacks(trait.id, stacks, null, null);
            }
            protected override UniTask Downgrade()
            {
                TableTraitListElement element = (TableTraitListElement)statDrawer.attached;
                Trait trait = element.Trait.Data;
                int stacks = card.Data.traits[trait.id].Stacks - 1;

                card.Data.traits.SetStacks(trait, stacks);
                upgradedTimes--;
                return element.List.SetStacks(trait.id, stacks, null, null);
            }

            protected override void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;

                TableTraitListElement element = ((TableTraitListElement)statDrawer.attached);
                Trait trait = element.Trait.Data;

                upgradePointsDelta = card.Data.PointsDeltaForTrait(trait, 1);
                if (upgradedTimes == 0)
                     downgradePointsDelta = 0;
                else downgradePointsDelta = card.Data.PointsDeltaForTrait(trait, -1);
                menu.UpgradeInfoShow(upgradePointsDelta, downgradePointsDelta);

                element.AdjustStacksInternal(1);
                menu.WriteUpgradedState(element.Trait.DescRich());
                element.AdjustStacksInternal(-1);
            }
            protected override void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                menu.UpgradeInfoHide();
                menu.WriteUpgradedState("");
            }
        }

        class InfoButtonDrawer : ButtonDrawer
        {
            public InfoButtonDrawer(CardUpgradeMenu menu) : base(menu, menu.Transform.Find("Info icon")) { }
            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                const string NOTES = "Нажмите ЛКМ или ПКМ по иконке характеристики, чтобы увеличить или уменьшить её значение.\n\n" +
                                     "Нажатие мышью на навык изменит изначальное количество его зарядов.\n\n" +
                                     "Доступные характеристики для изменения:\n- Стоимость\n- Инициатива\n- Здоровье\n- Сила\n- Заряды навыков";
                Tooltip.Show(NOTES);
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                Tooltip.Hide();
            }
        }
        class ResetAllButtonDrawer : ButtonDrawer
        {
            string _normalText;
            string _hoverText;

            public ResetAllButtonDrawer(CardUpgradeMenu menu) : base(menu, menu.Transform.Find("Left button")) 
            {
                UpdateTexts();
                menu.OnDeckPointsChanged += delta => UpdateTexts();
            }

            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                textMesh.text = _hoverText;
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                textMesh.text = _normalText;
            }
            protected override async void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
            {
                foreach (CardToUpgrade card in menu._sleeve.Cast<CardToUpgrade>())
                    await card.TryReset();
            }

            void UpdateTexts()
            {
                float defPoints = menu._sleeve.Cast<CardToUpgrade>().Sum(c => c.DefaultPoints).Rounded(0);
                float curPoints = menu._sleeve.Cast<CardToUpgrade>().Sum(c => c.CurrentPoints).Rounded(0);

                _normalText = $"СБРОСИТЬ ВСЁ<color=grey><size=75%>\nизначально: {defPoints} ОП\nсейчас: {curPoints} ОП";
                _hoverText = $"<u>СБРОСИТЬ ВСЁ</u><color=grey><size=75%>\nизначально: {defPoints} ОП\nсейчас: {curPoints} ОП";

                textMesh.text = _normalText;
            }
        }
        class ResetThisButtonDrawer : ButtonDrawer
        {
            string _normalText;
            string _hoverText;

            public ResetThisButtonDrawer(CardUpgradeMenu menu) : base(menu, menu.Transform.Find("Center button")) 
            {
                gameObject.SetActive(false);
                CardToUpgrade.OnSelectedChanged += UpdateTexts;
                menu.OnDeckPointsChanged += delta => UpdateTexts();
            }

            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected == null) return;
                textMesh.text = _hoverText;
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected == null) return;
                textMesh.text = _normalText;
            }
            protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected == null) return;
                CardToUpgrade.Selected.TryReset();
                textMesh.text = _normalText;
            }

            void UpdateTexts()
            {
                CardToUpgrade selected = CardToUpgrade.Selected;
                gameObject.SetActive(selected != null);
                if (selected == null) return;

                float defPoints = selected.DefaultPoints.Rounded(0);
                float curPoints = selected.CurrentPoints.Rounded(0);

                _normalText = $"СБРОСИТЬ КАРТУ<color=grey><size=75%>\nизначально: {defPoints} ОП\nсейчас: {curPoints} ОП";
                _hoverText = $"<u>СБРОСИТЬ КАРТУ</u><color=grey><size=75%>\nизначально: {defPoints} ОП\nсейчас: {curPoints} ОП";

                textMesh.text = _normalText;
            }
        }
        class FinishButtonDrawer : ButtonDrawer
        {
            const string NORMAL_TEXT = "ЗАВЕРШИТЬ >><color=grey><size=75%>\nпринять изменения";
            const string HOVER_OK_TEXT = "<u>ЗАВЕРШИТЬ >></u><color=grey><size=75%>\nпринять изменения";
            const string HOVER_ERR_TEXT = "<color=red><u>ЗАВЕРШИТЬ >></u><color=grey><size=75%>\nпринять изменения";

            public FinishButtonDrawer(CardUpgradeMenu menu) : base(menu, menu.Transform.Find("Right button")) { }

            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                if (!menu.CanLeave())
                     textMesh.text = HOVER_ERR_TEXT;
                else textMesh.text = HOVER_OK_TEXT;
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                textMesh.text = NORMAL_TEXT;
            }
            protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
            {
                if (menu.CanLeave())
                    menu.CloseAnimated();
            }
        }

        public CardUpgradeMenu(CardDeck deck, float pointsLimit) : base(ID, _prefab)
        {
            _pointsLimit = pointsLimit;
            _pointsCurrent = deck.Points;
            _sleeve = new SleeveToUpgrade(this, deck);
            _sleeve.TakeMissingCards(true);

            foreach (CardToUpgrade card in _sleeve.Cast<CardToUpgrade>())
                card.SetDefaults(card.Drawer.transform.position, card.CurrentPoints);

            _resetAllButton = new ResetAllButtonDrawer(this);
            _resetThisButton = new ResetThisButtonDrawer(this);
            _finishButton = new FinishButtonDrawer(this);
            _infoButton = new InfoButtonDrawer(this);

            _headerTextMesh = Transform.Find<TextMeshPro>("Header text");
            _descLeftTextMesh = Transform.Find<TextMeshPro>("Desc left");
            _descRightTextMesh = Transform.Find<TextMeshPro>("Desc right");

            _infoLeftTextMesh = Transform.Find<TextMeshPro>("Info left");
            _infoRightTextMesh = Transform.Find<TextMeshPro>("Info right");

            _arrows = new ArrowsAnim[]
            {
                new(Transform.Find("Arrows 1"), 40, -2.28f, 2.12f),
                new(Transform.Find("Arrows 2"), 60, 2.29f, -2.03f),
                new(Transform.Find("Arrows 3"), 80, -2.19f, 2.13f),
                new(Transform.Find("Arrows 4"), 30, 2.13f, -2.29f),
            };

            OnDeckPointsChanged += OnDeckPointsChangedBase;
            CardToUpgrade.OnSelectedChanged += UpgradeInfoHide;
            CardToUpgrade.OnAnyCardReset += delta => OnDeckPointsChanged(delta);
            CardToUpgrade.OnAnyStatGraded += delta => OnDeckPointsChanged(delta);

            OnDeckPointsChangedBase(0);
            _pointsAvailableAtStart = _pointsAvailable;
            UpgradeInfoHide();
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            Global.OnUpdate += CardToUpgrade.OnUpdate;
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Play();
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            Global.OnUpdate -= CardToUpgrade.OnUpdate;
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Kill();
        }

        public override void WriteDesc(string text)
        {
            base.WriteDesc(text);
            WriteCurrentState(text);
        }

        void WriteCurrentState(string desc)
        {
            if (desc != "")
                desc = desc.Insert(0, "<color=grey>// ТЕКУЩЕЕ СОСТОЯНИЕ //</color>\n\n");
            _descLeftTextMesh.text = desc;
        }
        void WriteUpgradedState(string desc)
        {
            if (desc != "")
                desc = desc.Insert(0, "<color=#408040>// БУДУЩЕЕ СОСТОЯНИЕ //</color>\n\n");
            _descRightTextMesh.text = desc;
        }

        void UpgradeInfoShow(float upgradedPointsDelta, float downgradedPointsDelta)
        {
            _infoLeftTextMesh.gameObject.SetActive(true);
            _infoRightTextMesh.gameObject.SetActive(true);

            string upgradePriceStr = (-upgradedPointsDelta.Rounded(0)).ToSignedNumberString();
            string downgradePriceStr = (-downgradedPointsDelta.Rounded(0)).ToSignedNumberString();

            _infoLeftTextMesh.text = $"УЛУЧШИТЬ:\n{upgradePriceStr} ОП";
            _infoRightTextMesh.text = $"УХУДШИТЬ:\n{downgradePriceStr} ОП";
            _infoRightTextMesh.color = downgradedPointsDelta == 0 ? Color.gray : Color.white;
        }
        void UpgradeInfoHide() 
        {
            _infoLeftTextMesh.gameObject.SetActive(false);
            _infoRightTextMesh.gameObject.SetActive(false);
        }

        void OnDeckPointsChangedBase(float pointsDelta)
        {
            _pointsCurrent += pointsDelta;
            _pointsAvailable = _pointsLimit - _pointsCurrent;
            RedrawHeader();
        }
        bool CanLeave()
        {
            return _pointsAvailable >= 0 || _pointsAvailable == _pointsAvailableAtStart;
        }
        void RedrawHeader()
        {
            float points = _pointsAvailable.Rounded(0);
            string pointsStr;

            if (points > 0)
                 pointsStr = $"<color=green>{points}</color>";
            else if (points < 0)
                 pointsStr = $"<color=red>{points}</color>";
            else pointsStr = points.ToString();
            _headerTextMesh.text = $"УЛУЧШИТЕ СВОИ КАРТЫ\n<size=75%>доступных очков прокачки: {pointsStr} ОП.";
        }
    }
}
