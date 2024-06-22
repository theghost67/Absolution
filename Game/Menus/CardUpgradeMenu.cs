using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Sleeves;
using Game.Territories;
using Game.Traits;
using GreenOne;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        const string NOTES = "Нажмите ЛКМ или ПКМ по иконке характеристики, чтобы увеличить или уменьшить её значение.\n\n" +
                             "Нажатие мышью на навык изменит изначальное количество его зарядов.\n\n" +
                             "Доступные характеристики для изменения:\n- Стоимость\n- Инициатива\n- Здоровье\n- Сила\n- Заряды навыков";

        const string FINISH_NORMAL_TEXT = "ЗАВЕРШИТЬ >><color=grey><size=75%>\nпринять изменения";
        const string FINISH_HOVER_OK_TEXT = "<u>ЗАВЕРШИТЬ >></u><color=grey><size=75%>\nпринять изменения";
        const string FINISH_HOVER_ERR_TEXT = "<color=red><u>ЗАВЕРШИТЬ >></u><color=grey><size=75%>\nпринять изменения";

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        public CardDeck Deck => _sleeve.Deck;

        public int PointsLimit => _pointsLimit;
        public int PointsCurrent => _pointsCurrent;

        readonly GameObject _leftButton;
        readonly GameObject _centerButton;
        readonly GameObject _rightButton;

        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _leftTextMesh;
        readonly TextMeshPro _centerTextMesh;
        readonly TextMeshPro _rightTextMesh;

        readonly TextMeshPro _descLeftTextMesh;
        readonly TextMeshPro _descRightTextMesh;

        readonly TextMeshPro _infoLeftTextMesh;
        readonly TextMeshPro _infoRightTextMesh;
        readonly Arrows[] _arrows;

        readonly SleeveToUpgrade _sleeve;
        readonly int _pointsLimit;
        int _pointsCurrent;
        int _pointsAvailable;

        // one of the field cards in this menu
        // note 1: all cards stay in the sleeve, even on selection
        // note 2: do not use Data.Points(), instead, use CurrentPoints property.
        class CardToUpgrade : TableFieldCard, ITableSleeveCard
        {
            const float ANIM_DURATION = 0.50f;
            public static CardToUpgrade Selected => _selected;

            public TableSleeve Sleeve => _sleeve;
            public ITableSleeveCard AsInSleeve => this;

            public Vector3 DefaultPos => _defaultPos;
            public int DefaultPoints => _defaultPoints;
            public int CurrentPoints => _currentPoints;

            bool ITableSleeveCard.IsInMove { get => _isPulling; set => _isPulling = value; }
            bool ITableSleeveCard.IsPulledOut { get => _isPulledOut; set => _isPulledOut = value; }

            static readonly Vector3 minScale = Vector3.one;
            static readonly Vector3 maxScale = Vector3.one * 1.25f;

            static CardToUpgrade _selected;
            readonly CardUpgradeMenu _menu;
            readonly TableSleeve _sleeve;
            readonly Dictionary<string, int> _upgradedStats; 

            Color _lastOutlineColor;
            Tween _scaleTween;
            Tween _posTween;

            Vector3 _defaultPos;
            int _defaultPoints;
            int _currentPoints;

            int _upgradePointsDelta;
            int _downgradePointsDelta;

            bool _isPulling;
            bool _isPulledOut;
            bool _awaitsUpgrade;

            public CardToUpgrade(CardUpgradeMenu menu, FieldCard data, TableSleeve sleeve) : base(data, sleeve.Drawer.transform) 
            { 
                _menu = menu;
                _sleeve = sleeve;

                _upgradedStats = new Dictionary<string, int>()
                {
                    { nameof(price), 0 },
                    { nameof(moxie), 0 },
                    { nameof(health), 0 },
                    { nameof(strength), 0 },
                };
                foreach (Trait trait in data.traits.Select(e => e.Trait))
                    _upgradedStats.Add(trait.id, 0);

                _scaleTween = Utils.emptyTween;
                _posTween = Utils.emptyTween;

                Drawer.ChangePointer = true;
                Drawer.OnMouseClickLeft += OnCardMouseClickLeft;

                Drawer.upperLeftIcon.OnMouseEnter += (s, e) => OnStatMouseEnter(nameof(price), s, e);
                Drawer.upperLeftIcon.OnMouseLeave += (s, e) => OnStatMouseLeave(nameof(price), s, e);
                Drawer.upperLeftIcon.OnMouseClickLeft += (s, e) => OnStatMouseClickLeft(nameof(price), s, e);
                Drawer.upperLeftIcon.OnMouseClickRight += (s, e) => OnStatMouseClickRight(nameof(price), s, e);

                Drawer.upperRightIcon.OnMouseEnter += (s, e) => OnStatMouseEnter(nameof(moxie), s, e);
                Drawer.upperRightIcon.OnMouseLeave += (s, e) => OnStatMouseLeave(nameof(moxie), s, e);
                Drawer.upperRightIcon.OnMouseClickLeft += (s, e) => OnStatMouseClickLeft(nameof(moxie), s, e);
                Drawer.upperRightIcon.OnMouseClickRight += (s, e) => OnStatMouseClickRight(nameof(moxie), s, e);

                Drawer.lowerLeftIcon.OnMouseEnter += (s, e) => OnStatMouseEnter(nameof(health), s, e);
                Drawer.lowerLeftIcon.OnMouseLeave += (s, e) => OnStatMouseLeave(nameof(health), s, e);
                Drawer.lowerLeftIcon.OnMouseClickLeft += (s, e) => OnStatMouseClickLeft(nameof(health), s, e);
                Drawer.lowerLeftIcon.OnMouseClickRight += (s, e) => OnStatMouseClickRight(nameof(health), s, e);

                Drawer.lowerRightIcon.OnMouseEnter += (s, e) => OnStatMouseEnter(nameof(strength), s, e);
                Drawer.lowerRightIcon.OnMouseLeave += (s, e) => OnStatMouseLeave(nameof(strength), s, e);
                Drawer.lowerRightIcon.OnMouseClickLeft += (s, e) => OnStatMouseClickLeft(nameof(strength), s, e);
                Drawer.lowerRightIcon.OnMouseClickRight += (s, e) => OnStatMouseClickRight(nameof(strength), s, e);

                foreach (TableTraitListElementDrawer drawer in Drawer.Traits)
                {
                    string id = drawer.attached.Trait.Data.id;
                    drawer.enqueueAnims = false;
                    drawer.OnMouseEnter += (s, e) => OnStatMouseEnter(id, s, e);
                    drawer.OnMouseLeave += (s, e) => OnStatMouseLeave(id, s, e);
                    drawer.OnMouseClickLeft += (s, e) => OnStatMouseClickLeft(id, s, e);
                    drawer.OnMouseClickRight += (s, e) => OnStatMouseClickRight(id, s, e);
                }
            }
            public static void OnUpdate()
            {
                if (_selected != null && Input.GetKeyDown(KeyCode.Escape))
                    _selected.Deselect(true);
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
            public void SetDefaults(Vector3 pos, int points)
            {
                _defaultPos = pos;
                _defaultPoints = points;
                _currentPoints = points;
            }
            public async UniTask Reset()
            {
                await ResetPrice();
                await ResetMoxie();
                await ResetHealth();
                await ResetStrength();

                IEnumerable<string> allIds = _upgradedStats.Keys;
                string[] statsIds = new string[]
                {
                    nameof(price),
                    nameof(moxie),
                    nameof(health),
                    nameof(strength),
                };
                IEnumerable<string> traitIds = allIds.Except(statsIds);

                foreach (string id in traitIds)
                    await ResetTrait(id);
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

            // invoke only if not selected
            async UniTask Select(bool setColliders)
            {
                Sleeve.Drawer.CanPullOut = false;

                if (setColliders)
                {
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.SetCollider(false);
                }

                _selected?.Deselect(!setColliders);
                _selected = this;
                _menu.RedrawResetCard();

                await AnimCardSelect().AsyncWaitForCompletion();

                if (setColliders)
                {
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.SetCollider(true);
                }
            }

            // invoke only if already selected
            async UniTask Deselect(bool setColliders)
            {
                _selected = null;
                _menu.RedrawResetCard();

                if (setColliders)
                {
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.SetCollider(false);
                }

                await AnimCardDeselect().AsyncWaitForCompletion();

                if (setColliders)
                {
                    foreach (ITableSleeveCard sleeveCard in _sleeve)
                        sleeveCard.Drawer.SetCollider(true);
                }

                Sleeve.Drawer.CanPullOut = true;
            }

            void OnCardMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                Drawer drawer = (TableFieldCardDrawer)sender;
                CardToUpgrade card = (CardToUpgrade)drawer.attached;
                if (_selected == this) return;

                card.Select(true);
            }
            async void OnStatMouseClickLeft(string statId, object sender, DrawerMouseEventArgs e) 
            {
                if (_awaitsUpgrade) return;
                if (_selected != this) return;
                if (_upgradePointsDelta + _menu._pointsAvailable < -9999) return;

                UniTask task = statId switch
                {
                    nameof(price) => UpgradePrice(),
                    nameof(moxie) => UpgradeMoxie(),
                    nameof(health) => UpgradeHealth(),
                    nameof(strength) => UpgradeStrength(),
                    _ => UpgradeTrait(statId),
                };

                if (_lastOutlineColor.a != 0 && Drawer.Outline.GetColorTween().IsPlaying())
                    Drawer.Outline.SetColor(_lastOutlineColor);
                _lastOutlineColor = Drawer.Outline.GetColor();
                Drawer.HighlightOutline(_lastOutlineColor, 0.5f);

                OnStatMouseEnter(statId, sender, e);
                _currentPoints += _upgradePointsDelta;
                _menu.OnDeckPointsChanged(_upgradePointsDelta);

                _awaitsUpgrade = true;
                await task;
                _awaitsUpgrade = false;
            }
            async void OnStatMouseClickRight(string statId, object sender, DrawerMouseEventArgs e)
            {
                if (_awaitsUpgrade) return;
                if (_selected != this) return;
                if (_upgradedStats[statId] <= 0) return;
                if (_downgradePointsDelta + _menu._pointsAvailable < -9999) return;

                UniTask task = statId switch
                {
                    nameof(price) => DowngradePrice(),
                    nameof(moxie) => DowngradeMoxie(),
                    nameof(health) => DowngradeHealth(),
                    nameof(strength) => DowngradeStrength(),
                    _ => DowngradeTrait(statId),
                };

                Drawer.HighlightOutline(Color.red, 0.5f);
                OnStatMouseEnter(statId, sender, e);
                _currentPoints += _downgradePointsDelta;
                _menu.OnDeckPointsChanged(_downgradePointsDelta);

                _awaitsUpgrade = true;
                await task;
                _awaitsUpgrade = false;
            }

            void OnStatMouseEnter(string statId, object sender, DrawerMouseEventArgs e)
            {
                if (_selected != this) return;
                _upgradePointsDelta = statId switch
                {
                    nameof(price) => Data.PointsDeltaForPrice(1),
                    nameof(moxie) => Data.PointsDeltaForMoxie(1),
                    nameof(health) => Data.PointsDeltaForHealth(1),
                    nameof(strength) => Data.PointsDeltaForStrength(1),
                    _ => Data.PointsDeltaForTrait(((TableTraitListElement)((Drawer)sender).attached).Trait.Data, 1),
                };
                _downgradePointsDelta = _upgradedStats[statId] <= 0 ? 0 : statId switch 
                {
                    nameof(price) => Data.PointsDeltaForPrice(-1),
                    nameof(moxie) => Data.PointsDeltaForMoxie(-1),
                    nameof(health) => Data.PointsDeltaForHealth(-1),
                    nameof(strength) => Data.PointsDeltaForStrength(-1),
                    _ => Data.PointsDeltaForTrait(((TableTraitListElement)((Drawer)sender).attached).Trait.Data, -1),
                };

                _menu.UpgradeInfoShow(_upgradePointsDelta, _downgradePointsDelta);
            }
            void OnStatMouseLeave(string statId, object sender, DrawerMouseEventArgs e)
            {
                if (_selected != this) return;
                _menu.UpgradeInfoHide();
            }

            UniTask UpgradePrice()
            {
                Data.price.value++;
                _upgradedStats[nameof(price)]++;
                return price.SetValueAbs(Data.price.value);
            }
            UniTask DowngradePrice()
            {
                Data.price.value--;
                _upgradedStats[nameof(price)]--;
                return price.SetValueAbs(Data.price.value);
            }
            UniTask ResetPrice()
            {
                Data.price.value -= _upgradedStats[nameof(price)];
                _upgradedStats[nameof(price)] = 0;
                return price.SetValueAbs(Data.price.value);
            }

            UniTask UpgradeMoxie()
            {
                Data.moxie++;
                _upgradedStats[nameof(moxie)]++;
                return moxie.SetValueAbs(Data.moxie);
            }
            UniTask DowngradeMoxie()
            {
                Data.moxie--;
                _upgradedStats[nameof(moxie)]--;
                return moxie.SetValueAbs(Data.moxie);
            }
            UniTask ResetMoxie()
            {
                Data.moxie -= _upgradedStats[nameof(moxie)];
                _upgradedStats[nameof(moxie)] = 0;
                return moxie.SetValueAbs(Data.moxie);
            }

            UniTask UpgradeHealth()
            {
                Data.health++;
                _upgradedStats[nameof(health)]++;
                return health.SetValueAbs(Data.health);
            }
            UniTask DowngradeHealth()
            {
                Data.health--;
                _upgradedStats[nameof(health)]--;
                return health.SetValueAbs(Data.health);
            }
            UniTask ResetHealth()
            {
                Data.health -= _upgradedStats[nameof(health)];
                _upgradedStats[nameof(health)] = 0;
                return health.SetValueAbs(Data.health);
            }

            UniTask UpgradeStrength()
            {
                Data.strength++;
                _upgradedStats[nameof(strength)]++;
                return strength.SetValueAbs(Data.strength);
            }
            UniTask DowngradeStrength()
            {
                Data.strength--;
                _upgradedStats[nameof(strength)]--;
                return strength.SetValueAbs(Data.strength);
            }
            UniTask ResetStrength()
            {
                Data.strength -= _upgradedStats[nameof(strength)];
                _upgradedStats[nameof(strength)] = 0;
                return strength.SetValueAbs(Data.strength);
            }

            UniTask UpgradeTrait(string id)
            {
                TableTraitListElement element = Drawer.Traits.elements[id].attached;
                Trait trait = element.Trait.Data;
                int stacks = Data.traits[id].Stacks + 1;

                Data.traits.SetStacks(trait, stacks);
                _upgradedStats[id]++;

                _menu.WriteDesc(element.Trait.DescRich());
                return element.List.AdjustStacks(trait.id, 1, null, null);
            }
            UniTask DowngradeTrait(string id)
            {
                TableTraitListElement element = Drawer.Traits.elements[id].attached;
                Trait trait = element.Trait.Data;
                int stacks = Data.traits[id].Stacks - 1;

                Data.traits.SetStacks(trait, stacks);
                _upgradedStats[id]--;

                _menu.WriteDesc(element.Trait.DescRich());
                return element.List.AdjustStacks(trait.id, -1, null, null);
            }
            UniTask ResetTrait(string id)
            {
                TableTraitListElement element = Drawer.Traits.elements[id].attached;
                Trait trait = element.Trait.Data;
                int stacks = Data.traits[id].Stacks - _upgradedStats[id];

                Data.traits.SetStacks(trait, stacks);
                _upgradedStats[id] = 0;

                _menu.WriteDesc(element.Trait.DescRich());
                return element.List.AdjustStacks(trait.id, 1, null, null);
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
        class Arrows
        {
            readonly Transform _transform;
            readonly Tween _tween;

            public Arrows(Transform transform, float duration, float startPos, float endPos)
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

        public CardUpgradeMenu(CardDeck deck, int pointsLimit) : base(ID, _prefab)
        {
            _pointsLimit = pointsLimit;
            _pointsCurrent = deck.Points;

            _leftButton = Transform.Find("Left button").gameObject;
            _centerButton = Transform.Find("Center button").gameObject;
            _rightButton = Transform.Find("Right button").gameObject;

            _headerTextMesh = Transform.Find<TextMeshPro>("Header text");
            _leftTextMesh = _leftButton.GetComponent<TextMeshPro>();
            _centerTextMesh = _centerButton.GetComponent<TextMeshPro>();
            _rightTextMesh = _rightButton.GetComponent<TextMeshPro>();

            _descLeftTextMesh = Transform.Find<TextMeshPro>("Desc left");
            _descRightTextMesh = Transform.Find<TextMeshPro>("Desc right");

            _infoLeftTextMesh = Transform.Find<TextMeshPro>("Info left");
            _infoRightTextMesh = Transform.Find<TextMeshPro>("Info right");

            _arrows = new Arrows[]
            {
                new(Transform.Find("Arrows 1"), 40, -2.28f, 2.12f),
                new(Transform.Find("Arrows 2"), 60, 2.29f, -2.03f),
                new(Transform.Find("Arrows 3"), 80, -2.19f, 2.13f),
                new(Transform.Find("Arrows 4"), 30, 2.13f, -2.29f),
            };

            _leftButton.SetActive(false);
            _centerButton.SetActive(false);

            _sleeve = new SleeveToUpgrade(this, deck);
            _sleeve.TakeMissingCards(true);

            foreach (ITableSleeveCard card in _sleeve)
                ((CardToUpgrade)card).SetDefaults(card.Drawer.transform.position, card.Data.Points());

            Transform infoIcon = Transform.Find("Info icon");
            Drawer infoDrawer = new(null, infoIcon);
            infoDrawer.OnMouseEnter += OnInfoIconMouseEnter;
            infoDrawer.OnMouseLeave += OnInfoIconMouseLeave;

            Drawer resetAllDrawer = new(null, _leftButton);
            resetAllDrawer.OnMouseClickLeft += OnResetAllButtonMouseClickLeft;
            infoDrawer.OnMouseLeave += OnInfoIconMouseLeave;

            Drawer resetCardDrawer = new(null, _centerButton);
            resetCardDrawer.OnMouseClickLeft += OnResetCardButtonMouseClickLeft;

            Drawer finishDrawer = new(null, _rightButton);
            finishDrawer.OnMouseEnter += OnFinishButtonMouseEnter;
            finishDrawer.OnMouseLeave += OnFinishButtonMouseLeave;
            finishDrawer.OnMouseClickLeft += OnFinishButtonMouseClickLeft;

            RedrawHeader();
            UpgradeInfoHide();
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            Global.OnUpdate += CardToUpgrade.OnUpdate;
            foreach (Arrows arrows in _arrows)
                arrows.Play();
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            Global.OnUpdate -= CardToUpgrade.OnUpdate;
            foreach (Arrows arrows in _arrows)
                arrows.Kill();
        }

        // TODO: replace by creating own TableTrait MouseEnter/Left implementation (create virtual bool ShowTooltip in TableTraitDrawer/TableCardDrawer)
        public override void WriteDesc(string text)
        {
            if (text != "")
                text = text.Insert(0, "<color=grey>// ТЕКУЩЕЕ СОСТОЯНИЕ //</color>\n\n");

            base.WriteDesc(text);
            WriteLeftDesc(text);
        }

        void RedrawHeader()
        {
            string pointsStr;
            if (_pointsAvailable > 0)
                 pointsStr = $"<color=green>{_pointsAvailable}</color>";
            else if (_pointsAvailable < 0)
                 pointsStr = $"<color=red>{_pointsAvailable}</color>";
            else pointsStr = _pointsAvailable.ToString();
            _headerTextMesh.text = $"УЛУЧШИТЕ СВОИ КАРТЫ\n<size=75%>доступных очков прокачки: {pointsStr} ОП.";
        }
        void RedrawResetCard()
        {
            CardToUpgrade selected = CardToUpgrade.Selected;
            bool hasSelected = selected != null;
            _centerButton.SetActive(hasSelected);
            if (!hasSelected) return;

            int defPoints = selected.DefaultPoints;
            int curPoints = selected.CurrentPoints;

            _centerTextMesh.text = $"СБРОСИТЬ КАРТУ<size=75%><color=grey>\nизначально: {defPoints} очков\nсейчас: {curPoints} очков";
        }

        void WriteLeftDesc(string text)
        {
            _descLeftTextMesh.text = text;
        }
        void WriteRightDesc(string text)
        {
            _descRightTextMesh.text = text;
        }

        void UpgradeInfoShow(int upgradedPointsDelta, int downgradedPointsDelta)
        {
            _infoLeftTextMesh.gameObject.SetActive(true);
            _infoRightTextMesh.gameObject.SetActive(true);

            _infoLeftTextMesh.text = $"УЛУЧШИТЬ:\n{upgradedPointsDelta} ОП";
            _infoRightTextMesh.text = $"УХУДШИТЬ:\n{downgradedPointsDelta} ОП";
            _infoRightTextMesh.color = downgradedPointsDelta == 0 ? Color.gray : Color.white;
        }
        void UpgradeInfoHide() 
        {
            _infoLeftTextMesh.gameObject.SetActive(false);
            _infoRightTextMesh.gameObject.SetActive(false);
        }

        // invokes after any upgrade/downgrade
        void OnDeckPointsChanged(int pointsDelta)
        {
            _pointsCurrent += pointsDelta;
            _pointsAvailable = _pointsLimit - _pointsCurrent;
            RedrawHeader();
            RedrawResetCard();
        }

        void OnInfoIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            Tooltip.Show(NOTES);
        }
        void OnInfoIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            Tooltip.Hide();
        }

        void OnFinishButtonMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (_pointsCurrent > _pointsLimit)
                 _rightTextMesh.text = FINISH_HOVER_ERR_TEXT;
            else _rightTextMesh.text = FINISH_HOVER_OK_TEXT;
        }
        void OnFinishButtonMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            _rightTextMesh.text = FINISH_NORMAL_TEXT;
        }

        void OnResetAllButtonMouseClickLeft(object sender, DrawerMouseEventArgs e)
        {

        }
        void OnResetCardButtonMouseClickLeft(object sender, DrawerMouseEventArgs e)
        {

        }
        void OnFinishButtonMouseClickLeft(object sender, DrawerMouseEventArgs e)
        {
            e.handled |= _pointsCurrent > _pointsLimit;
            if (e.handled) return;
            CloseAnimated();
        }
    }
}
