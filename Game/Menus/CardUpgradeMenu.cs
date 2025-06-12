using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Sleeves;
using Game.Territories;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню с возможностью улучшать карты поля колоды игрока.
    /// </summary>
    public class CardUpgradeMenu : Menu
    {
        const string ID = "card_upgrade";

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        public event Action<float> OnDeckPointsChanged;
        public event Action<float> OnAnyStatGraded; // float = delta
        public event Action<float> OnAnyCardReset;
        public event Action OnSelectedChanged;

        public CardDeck Deck => _sleeve.Deck;
        public float PointsLimit => _pointsLimit;
        public float PointsCurrent => _pointsCurrent;
        public float PointsAvailable => _pointsAvailable;
        public override string LinkedMusicMixId => "peace";

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

            public TableSleeve Sleeve => _sleeve;
            public ITableSleeveCard AsInSleeve => this;

            public Vector3 DefaultPos => _defaultPos;
            public float DefaultPoints => _defaultPoints;
            public float CurrentPoints => Data.Points() - _defaultPoints;

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
            int _defaultSorting;

            public CardToUpgrade(CardUpgradeMenu menu, FieldCard data, TableSleeve sleeve) : base(data, sleeve.Drawer?.transform) 
            { 
                _menu = menu;
                _sleeve = sleeve;
                _stats = new StatToUpgrade[4 + Data.traits.Count];
                TryOnInstantiatedAction(GetType(), typeof(CardToUpgrade));
            }
            public static void OnUpdate()
            {
                if (_selected != null && Input.GetKeyDown(KeyCode.Escape))
                    _ = _selected.Deselect(false);
            }

            public bool CanTake() => false;
            public bool CanReturn() => false;
            public bool CanDropOn(TableField field) => false;
            public bool CanPullOut() => _selected != this && Drawer.ColliderEnabled;
            public bool CanPullIn() => _selected != this && Drawer.ColliderEnabled;

            public void OnTake() => AsInSleeve.OnTakeBase();
            public void OnReturn() => AsInSleeve.OnReturnBase();
            public void OnDropOn(TableSleeveCardDropArgs e) => AsInSleeve.OnDropOnBase(e);
            public Tween OnPullOut(bool sleevePull)
            {
                if (_selected != this)
                     return AsInSleeve.OnPullOutBase(sleevePull);
                else return null;
            }
            public Tween OnPullIn(bool sleevePull)
            {
                if (_selected != this)
                     return AsInSleeve.OnPullInBase(sleevePull);
                else return null;
            }

            // set once sleeve drawer created
            public void SetDefaults(Vector3 pos, float points, int sortingOrder)
            {
                _defaultPos = pos;
                _defaultPoints = points;
                _defaultSorting = sortingOrder;
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

            public async UniTask TryReset()
            {
                if (!ResetAvailable()) return;
                float startPoints = CurrentPoints;

                foreach (StatToUpgrade stat in _stats)
                    await stat.TryReset();

                float pointsDelta = CurrentPoints - startPoints;
                _menu.OnAnyCardReset?.Invoke(pointsDelta);
            }
            public void TryReselect()
            {
                foreach (StatToUpgrade stat in _stats)
                    stat.TryReselect();
            }

            public void AnimHighlightAsUpgrade()
            {
                Drawer.AnimHighlightOutline(0.5f);
            }
            public void AnimHighlightAsDowngrade()
            {
                Drawer.AnimHighlightOutline(0.5f, new Color(1.0f, 0.2f, 0.2f));
            }

            protected override void OnDrawerCreatedBase(object sender, EventArgs e)
            {
                base.OnDrawerCreatedBase(sender, e);

                _stats[0] = new PriceStatToUpgrade(_menu, this, Drawer.priceIcon);
                _stats[1] = new MoxieStatToUpgrade(_menu, this, Drawer.moxieIcon);
                _stats[2] = new HealthStatToUpgrade(_menu, this, Drawer.healthIcon);
                _stats[3] = new StrengthStatToUpgrade(_menu, this, Drawer.strengthIcon);
                int index = 4;
                foreach (Trait trait in Data.traits.Select(e => e.Trait))
                    _stats[index++] = new TraitStatToUpgrade(_menu, this, Drawer.Traits.queue[trait.id]);
                foreach (StatToUpgrade stat in _stats)
                    stat.OnGraded += delta => _menu.OnAnyStatGraded(delta);

                Drawer.ChangePointer = true;
                Drawer.OnMouseClick += (s, e) => { if (_selected != this) _ = Select(); };
            }
            protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
            {
                base.OnDrawerDestroyedBase(sender, e);
                Dispose();
            }

            Tween AnimCardSelect(Vector3 from, Vector3 to)
            {
                Drawer.transform.position = from;
                Sequence seq = DOTween.Sequence(Drawer);
                seq.AppendCallback(() =>
                {
                    AnimScaleUp();
                    AnimPosUp(to);
                });
                seq.AppendInterval(ANIM_DURATION);
                seq.OnComplete(() => Drawer.ColliderEnabled = true);

                return seq.Play();
            }
            Tween AnimCardDeselect(Vector3 from, Vector3 to)
            {
                Drawer.transform.position = from;
                Sequence seq = DOTween.Sequence(Drawer);
                seq.AppendCallback(() =>
                {
                    AnimScaleDown();
                    AnimPosDown(to);
                });
                seq.AppendInterval(ANIM_DURATION);
                seq.OnComplete(() => Drawer.ColliderEnabled = true);

                return seq.Play();
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
            Tween AnimPosUp(Vector3 to)
            {
                _posTween.Kill();
                _posTween = Drawer.transform.DOMove(to, ANIM_DURATION).SetEase(Ease.OutQuad);
                return _posTween;
            }
            Tween AnimPosDown(Vector3 to)
            {
                _posTween.Kill();
                _posTween = Drawer.transform.DOMove(to, ANIM_DURATION).SetEase(Ease.InQuad);
                return _posTween;
            }

            async UniTask Select()
            {
                if (Drawer == null || Drawer.IsDestroying) return;

                Vector3 prevPos = Drawer.transform.position;
                Sleeve.Drawer.CanPullOut = false;
                //Sleeve.Remove(this);

                foreach (ITableSleeveCard sleeveCard in _sleeve)
                    sleeveCard.Drawer.ColliderEnabled = false;

                _selected?.Deselect(true);
                _selected = this;
                _menu.OnSelectedChanged?.Invoke();

                Vector3 from = prevPos;
                Vector3 to = Vector3.zero;
                await AnimCardSelect(from, to).AsyncWaitForCompletion();

                _selected.Drawer.SortingOrderDefault = 0;
                foreach (ITableSleeveCard sleeveCard in _sleeve)
                    sleeveCard.Drawer.ColliderEnabled = true;

                Sleeve.Drawer.CanPullOut = true;
            }
            async UniTask Deselect(bool insideOfSelect)
            {
                if (Drawer == null || Drawer.IsDestroying) return;

                _selected.Drawer.SortingOrderDefault = _defaultSorting;
                _selected = null;
                //Sleeve.Add(this); // moves Drawer.transform

                if (!insideOfSelect)
                {
                    _menu.OnSelectedChanged?.Invoke();
                    Sleeve.Drawer.CanPullOut = false;
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.ColliderEnabled = false;
                }

                Vector3 from = Vector3.zero;
                Vector3 to = _defaultPos;
                await AnimCardDeselect(from, to).AsyncWaitForCompletion();

                if (!insideOfSelect)
                {
                    Sleeve.Drawer.CanPullOut = true;
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.ColliderEnabled = true;
                }
            }
        }
        class SleeveToUpgrade : TableSleeve
        {
            readonly CardUpgradeMenu _menu;
            public SleeveToUpgrade(CardUpgradeMenu menu, CardDeck deck) : base(deck, true, menu.Transform)
            { 
                _menu = menu;
                TryOnInstantiatedAction(GetType(), typeof(SleeveToUpgrade));
            }
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
            public int UpgradedTimes => _upgradedTimes;
            public readonly CardToUpgrade card;
            protected readonly CardUpgradeMenu menu;
            protected readonly Drawer statDrawer;
            int _upgradedTimes;

            public StatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer)
            {
                this.menu = menu;
                this.card = card;
                this.statDrawer = statDrawer;

                statDrawer.OnMouseEnter += OnMouseEnter;
                statDrawer.OnMouseLeave += OnMouseLeave;
                statDrawer.OnMouseClick += OnMouseClick;
            }
            public UniTask TryReset()
            {
                if (_upgradedTimes != 0)
                     return Reset();
                else return UniTask.CompletedTask;
            }
            public void TryReselect()
            {
                if (statDrawer.IsSelected)
                    statDrawer.IsSelected = true;
            }

            protected virtual UniTask Reset()
            {
                _upgradedTimes = 0;
                return UniTask.CompletedTask;
            }
            protected virtual UniTask Upgrade(int times)
            {
                _upgradedTimes += times;
                return UniTask.CompletedTask;
            }
            protected virtual UniTask Downgrade(int times)
            {
                _upgradedTimes -= times;
                return UniTask.CompletedTask;
            }

            protected abstract float GetGradePointsDelta(int times, bool downgrade);
            protected virtual bool UpgradeIsPossible() => true;
            protected static int GetUserGradeTimes()
            {
                int times = 1;
                if (Input.GetKey(KeyCode.LeftShift))
                    times *= 5;
                if (Input.GetKey(KeyCode.LeftControl))
                    times *= 5;
                return times;
            }

            void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                int times = GetUserGradeTimes();
                float upgradePointsDelta = GetGradePointsDelta(times, false);
                float downgradePointsDelta = times > _upgradedTimes ? 0 : GetGradePointsDelta(times, true);
                menu.UpgradeInfoShow(upgradePointsDelta, downgradePointsDelta);
            }
            void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                menu.UpgradeInfoHide();
            }
            void OnMouseClick(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                if (e.isLmbDown)
                     OnMouseClickLeft(sender, e);
                else OnMouseClickRight(sender, e);
            }

            async void OnMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                if (!UpgradeIsPossible()) return;

                int upgradeTimes = GetUserGradeTimes();
                float upgradePointsDelta = GetGradePointsDelta(upgradeTimes, false);
                if (upgradePointsDelta > 0 && menu._pointsAvailable + upgradePointsDelta < -9999) 
                    return;

                card.AnimHighlightAsUpgrade();
                menu.SetCollider(false);
                await Upgrade(upgradeTimes);
                OnGraded?.Invoke(upgradePointsDelta);
                menu.SetCollider(true);
                statDrawer.IsSelected = true; // force mouse enter invoke
                e.handled = true;
            }
            async void OnMouseClickRight(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected != card) return;
                if (_upgradedTimes <= 0) return;

                int downgradeTimes = GetUserGradeTimes();
                if (downgradeTimes > _upgradedTimes) return;
                float downgradePointsDelta = GetGradePointsDelta(downgradeTimes, true);
                if (downgradePointsDelta > 0 && menu._pointsAvailable - downgradePointsDelta < -9999) return;

                card.AnimHighlightAsDowngrade();
                menu.SetCollider(false);
                await Downgrade(downgradeTimes);
                OnGraded?.Invoke(downgradePointsDelta);
                menu.SetCollider(true);
                statDrawer.IsSelected = true; // force mouse enter invoke
                e.handled = true;
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
            protected override async UniTask Reset()
            {
                int times = UpgradedTimes;
                await base.Reset();
                card.Data.price.value -= times;
                await card.Price.SetValue(card.Data.price.value, null);
            }
            protected override async UniTask Upgrade(int times)
            {
                await base.Upgrade(times);
                card.Data.price.value += times;
                await card.Price.SetValue(card.Data.price.value, null);
            }
            protected override async UniTask Downgrade(int times)
            {
                await base.Downgrade(times);
                card.Data.price.value -= times;
                await card.Price.SetValue(card.Data.price.value, null);
            }
            protected override float GetGradePointsDelta(int times, bool downgrade)
            {
                return 0;
            }
            protected override bool UpgradeIsPossible() => false;
        }
        class MoxieStatToUpgrade : StatToUpgrade
        {
            public MoxieStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) { }
            protected override async UniTask Reset()
            {
                int times = UpgradedTimes;
                await base.Reset();
                card.Data.moxie -= times;
                await card.Moxie.SetValue(card.Data.moxie, null);
            }
            protected override async UniTask Upgrade(int times)
            {
                await base.Upgrade(times);
                card.Data.moxie += times;
                await card.Moxie.SetValue(card.Data.moxie, null);
            }
            protected override async UniTask Downgrade(int times)
            {
                await base.Downgrade(times);
                card.Data.moxie -= times;
                await card.Moxie.SetValue(card.Data.moxie, null);
            }
            protected override float GetGradePointsDelta(int times, bool downgrade)
            {
                if (downgrade)
                    return card.Data.PointsDeltaForMoxie(-times);
                else return card.Data.PointsDeltaForMoxie(times);
            }
            protected override bool UpgradeIsPossible() => card.Moxie < 5;
        }
        class HealthStatToUpgrade : StatToUpgrade
        {
            public HealthStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) { }
            protected override async UniTask Reset()
            {
                int times = UpgradedTimes;
                await base.Reset();
                card.Data.health -= times;
                await card.Health.SetValue(card.Data.health, null);
            }
            protected override async UniTask Upgrade(int times)
            {
                await base.Upgrade(times);
                card.Data.health += times;
                await card.Health.SetValue(card.Data.health, null);
            }
            protected override async UniTask Downgrade(int times)
            {
                await base.Downgrade(times);
                card.Data.health -= times;
                await card.Health.SetValue(card.Data.health, null);
            }
            protected override float GetGradePointsDelta(int times, bool downgrade)
            {
                if (downgrade)
                    return card.Data.PointsDeltaForHealth(-times);
                else return card.Data.PointsDeltaForHealth(times);
            }
        }
        class StrengthStatToUpgrade : StatToUpgrade
        {
            public StrengthStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, Drawer statDrawer) : base(menu, card, statDrawer) {  }
            protected override async UniTask Reset()
            {
                int times = UpgradedTimes;
                await base.Reset();
                card.Data.strength -= times;
                await card.Strength.SetValue(card.Data.strength, null);
            }
            protected override async UniTask Upgrade(int times)
            {
                await base.Upgrade(times);
                card.Data.strength += times;
                await card.Strength.SetValue(card.Data.strength, null);
            }
            protected override async UniTask Downgrade(int times)
            {
                await base.Downgrade(times);
                card.Data.strength -= times;
                await card.Strength.SetValue(card.Data.strength, null);
            }
            protected override float GetGradePointsDelta(int times, bool downgrade)
            {
                if (downgrade)
                    return card.Data.PointsDeltaForStrength(-times);
                else return card.Data.PointsDeltaForStrength(times);
            }
        }
        class TraitStatToUpgrade : StatToUpgrade
        {
            readonly TableTraitListElement _element;
            readonly Trait _trait;
            readonly string _traitId;

            public TraitStatToUpgrade(CardUpgradeMenu menu, CardToUpgrade card, TableTraitListElementDrawer statDrawer) : base(menu, card, statDrawer)
            {
                statDrawer.enqueueAnims = false;
                _element = statDrawer.attached;
                _element.Drawer.OnMouseEnter += OnMouseEnter;
                _element.Drawer.OnMouseLeave += OnMouseLeave;
                _trait = _element.Trait.Data;
                _traitId = _trait.id;
            }

            protected override async UniTask Reset()
            {
                int times = UpgradedTimes;
                await base.Reset();

                int stacks = card.Data.traits[_traitId].Stacks - times;
                card.Data.traits.SetStacks(_trait, stacks);

                _element.Drawer.AnimAdjust(stacks);
                await _element.List.SetStacks(_traitId, stacks, null, null);
            }
            protected override async UniTask Upgrade(int times)
            {
                await base.Upgrade(times);
                int stacks = card.Data.traits[_traitId].Stacks + times;
                card.Data.traits.SetStacks(_trait, stacks);
                await _element.List.SetStacks(_traitId, stacks, null, null);
            }
            protected override async UniTask Downgrade(int times)
            {
                await base.Downgrade(times);
                int stacks = card.Data.traits[_traitId].Stacks - times;
                card.Data.traits.SetStacks(_trait, stacks);
                await _element.List.SetStacks(_traitId, stacks, null, null);
            }

            protected override float GetGradePointsDelta(int times, bool downgrade)
            {
                if (!UpgradeIsPossible())
                    return 0;
                if (downgrade)
                     return card.Data.PointsDeltaForTrait(_trait, -times);
                else return card.Data.PointsDeltaForTrait(_trait, times);
            }
            protected override bool UpgradeIsPossible()
            {
                return !_trait.tags.HasFlag(TraitTag.Static);
            }

            void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (_element.Trait.Data.tags.HasFlag(TraitTag.Static))
                    return;
                int times = GetUserGradeTimes();
                menu.WriteCurrentState(_element.Trait.DescDynamic(out _));
                _element.AdjustStacksInternal(times);
                string desc = _element.Trait.DescDynamicWithLinks(out string[] descLinksTexts);
                menu.WriteUpgradedState(desc);
                Tooltip.SetAlign(HorizontalAlignmentOptions.Left);
                Tooltip.SetText(descLinksTexts.Prepend(desc).ToArray());
                _element.AdjustStacksInternal(-times);
            }
            void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (_element.Trait.Data.tags.HasFlag(TraitTag.Static))
                    return;
                menu.WriteCurrentState("");
                menu.WriteUpgradedState("");
                Tooltip.ClearText();
            }
        }

        class InfoButtonDrawer : Drawer
        {
            public InfoButtonDrawer(CardUpgradeMenu menu) : base(menu, menu.Transform.Find("Info button"))
            {
                const string TEXT = "Нажмите ЛКМ или ПКМ по иконке характеристики, чтобы увеличить или уменьшить её значение. " +
                                    "Нажатие мышью на навык изменит изначальное количество его зарядов. " +
                                    "Зажатие клавиш Ctrl/Shift увеличит количество совершаемых улучшений за раз. " +
                                    "\n\nДоступные характеристики для изменения:\n- Инициатива\n- Здоровье\n- Сила";
                SetTooltip(TEXT);
                SetTooltipAlign(HorizontalAlignmentOptions.Right);
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
            protected override async void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
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
                menu.OnSelectedChanged += UpdateTexts;
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
            protected override async void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                if (CardToUpgrade.Selected == null || !e.isLmbDown) return;
                await CardToUpgrade.Selected.TryReset();
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
            protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                if (menu.CanLeave() || !e.isLmbDown)
                    menu.TransitFromThis();
            }
        }

        public CardUpgradeMenu(CardDeck deck, float pointsLimit) : base(ID, _prefab)
        {
            _pointsLimit = pointsLimit;
            _pointsCurrent = 0;
            _sleeve = new SleeveToUpgrade(this, deck);
            _sleeve.TakeMissingCards(true);

            foreach (CardToUpgrade card in _sleeve.Cast<CardToUpgrade>())
                card.SetDefaults(card.Drawer.transform.position, card.CurrentPoints, card.Drawer.SortingOrder);

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
            OnSelectedChanged += UpgradeInfoHide;
            OnAnyCardReset += delta => OnDeckPointsChanged(delta);
            OnAnyStatGraded += delta => OnDeckPointsChanged(delta);

            OnDeckPointsChangedBase(0);
            _pointsAvailableAtStart = _pointsAvailable;
            UpgradeInfoHide();
        }

        protected override void Open()
        {
            base.Open();
            Global.OnUpdate += OnUpdate;
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Play();
        }
        protected override void Close()
        {
            base.Close();
            Global.OnUpdate -= OnUpdate;
            foreach (ArrowsAnim arrows in _arrows)
                arrows.Kill();
        }
        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            _infoButton.ColliderEnabled = value;
            _resetAllButton.ColliderEnabled = value;
            _resetThisButton.ColliderEnabled = value;
            _finishButton.ColliderEnabled = value;
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

            string upgradePriceStr = (-upgradedPointsDelta.Clamped(-9999, 9999).Rounded(0)).ToSignedString();
            string downgradePriceStr = (-downgradedPointsDelta.Clamped(-9999, 9999).Rounded(0)).ToSignedString();

            _infoLeftTextMesh.text = $"УЛУЧШИТЬ:\n{upgradePriceStr} ОП";
            _infoLeftTextMesh.color = upgradedPointsDelta == 0 ? Color.gray : Color.white;

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
        void OnUpdate()
        {
            CardToUpgrade.OnUpdate();
            if (CardToUpgrade.Selected == null) return;
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftControl) ||
                Input.GetKeyUp(KeyCode.LeftShift)   || Input.GetKeyUp(KeyCode.LeftControl))
                CardToUpgrade.Selected.TryReselect();
        }

        bool CanLeave()
        {
            return _pointsAvailable >= 0 || _pointsAvailable.Rounded(1) == _pointsAvailableAtStart.Rounded(1);
        }
        void RedrawHeader()
        {
            float points = _pointsAvailable.Rounded(0);
            string pointsStr;

            if (points == 0)
            {
                if (_pointsAvailable >= 0)
                     pointsStr = $"<color=green>>0</color>";
                else pointsStr = $"<color=red><0</color>";
            }
            else if (_pointsAvailable > 0)
                 pointsStr = $"<color=green>{points}</color>";
            else if (_pointsAvailable < 0)
                 pointsStr = $"<color=red>{points}</color>";
            else pointsStr = points.ToString();
            _headerTextMesh.text = $"УЛУЧШИТЕ СВОИ КАРТЫ\n<size=75%>доступных очков прокачки: {pointsStr} ОП.";
        }
    }
}
