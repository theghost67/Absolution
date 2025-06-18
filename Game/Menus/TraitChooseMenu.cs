using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Sleeves;
using Game.Territories;
using Game.Traits;
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
    public class TraitChooseMenu : Menu
    {
        const string ID = "trait_choose";
        const int MAX_TRAITS = 5;

        const float SELECTED_CARD_TEXT_Y_UPPER  = -0.9f * Global.PIXEL_SCALE;
        const float SELECTED_CARD_TEXT_Y_NORMAL = -1 * Global.PIXEL_SCALE;

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        public bool TraitsAreShown => _traitsAreShown;
        public override string LinkedMusicMixId => "peace";

        public int TraitsCount => _traitsPerChoice;
        public int RerollsLeft => _rerollsLeft;
        public int ChoicesLeft => _choicesLeft;

        readonly Transform _traitsParent;
        readonly TextMeshPro _selectedCardText;
        readonly TextMeshPro _headerTextMesh;
        readonly RerollButtonDrawer _rerollButton;
        readonly DeclineButtonDrawer _declineButton;
        readonly ArrowsAnim[] _arrows;
        readonly SleeveToChoose _sleeve;

        int _choicesLeft;
        int _traitsPerChoice;
        int _rerollsLeft;
        bool _traitsAreShown;
        bool _animInProgress;
        TraitToChoose[] _traits;
        CardToChoose _chosenCard;

        class CardToChoose : TableFieldCard, ITableSleeveCard
        {
            public static CardToChoose Selected => _selected;

            public TableSleeve Sleeve => _sleeve;
            public ITableSleeveCard AsInSleeve => this;

            static CardToChoose _selected;
            readonly TraitChooseMenu _menu;
            readonly TableSleeve _sleeve;

            public CardToChoose(TraitChooseMenu menu, FieldCard data, TableSleeve sleeve) : base(data, sleeve.Drawer?.transform)
            {
                _menu = menu;
                _sleeve = sleeve;
                TryOnInstantiatedAction(GetType(), typeof(CardToChoose));
            }
            public static void OnUpdate()
            {
                if (_selected != null && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
                    _selected.Deselect(false);
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
                return AsInSleeve.OnPullOutBase(sleevePull);
            }
            public Tween OnPullIn(bool sleevePull)
            {
                return AsInSleeve.OnPullInBase(sleevePull);
            }

            protected override void OnDrawerCreatedBase(object sender, EventArgs e)
            {
                base.OnDrawerCreatedBase(sender, e);
                Drawer.ChangePointer = true;
                Drawer.OnMouseClick += (s, e) => { if (_selected != this) Select(); };
            }
            protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
            {
                base.OnDrawerDestroyedBase(sender, e);
                Dispose();
            }

            public void Select()
            {
                if (Drawer == null || Drawer.IsDestroying) return;
                Drawer.AnimHighlightOutline(0.5f);
                Sleeve.Drawer.CanPullOut = false;
                _selected?.Deselect(true);
                _selected = this;
                OnPullOut(false);
                _menu.SelectCard(this);
            }
            public void Deselect(bool insideOfSelect)
            {
                if (Selected == null || Drawer == null || Drawer.IsDestroying) return;

                _selected?.OnPullIn(false);
                _selected = null;
                _menu.DeselectCard();
                if (!insideOfSelect)
                    Sleeve.Drawer.CanPullOut = true;
            }
        }
        class SleeveToChoose : TableSleeve
        {
            readonly TraitChooseMenu _menu;
            public SleeveToChoose(TraitChooseMenu menu, CardDeck deck) : base(deck, true, menu.Transform)
            {
                _menu = menu;
                TryOnInstantiatedAction(GetType(), typeof(SleeveToChoose));
            }
            protected override ITableSleeveCard HoldingCardCreator(Card data)
            {
                if (data.isField)
                    return new CardToChoose(_menu, (FieldCard)data, this);
                else return null;
            }
        }

        class TraitToChoose
        {
            const float ANIM_DURATION = 0.33f;
            const float CARD_DISTANCE_X = 1f;

            const float CARD_MIN_Y = -0.25f;
            const float CARD_MAX_Y = 0.00f;

            static readonly Vector3 minScale = Vector3.one * 2.5f;
            static readonly Vector3 maxScale = Vector3.one * 3.0f;

            readonly TraitChooseMenu _menu;
            readonly TableTrait _onTable;

            public TableTraitDrawer Drawer => _onTable.Drawer;
            public Trait Data => _onTable.Data;

            bool _chosen;
            Tween _scaleTween;
            Tween _posTween;
            Tween _alphaTween;

            public TraitToChoose(TraitChooseMenu menu, TableTrait onTable, int index)
            {
                _menu = menu;
                _onTable = onTable;

                float xHalfOffset = -menu._traitsPerChoice / 2f + 0.5f;
                TableTraitDrawer drawer = _onTable.Drawer;

                drawer.transform.localPosition = new Vector3(index * CARD_DISTANCE_X + xHalfOffset, CARD_MIN_Y);
                drawer.ChangePointer = true;
                drawer.transform.localScale = Vector3.one * 2.5f;

                drawer.OnMouseEnter += OnTraitMouseEnter;
                drawer.OnMouseLeave += OnTraitMouseLeave;
                drawer.OnMouseClick += OnTraitMouseClickLeft;

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

            void OnTraitMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                AnimScaleUp();
                string desc = _onTable.DescDynamicWithLinks(out string[] links);
                Tooltip.SetAlign(HorizontalAlignmentOptions.Left);
                Tooltip.SetText(links.Prepend(desc).ToArray());
            }
            void OnTraitMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                AnimScaleDown();
                Tooltip.ClearText();
            }
            void OnTraitMouseClickLeft(object sender, DrawerMouseEventArgs e)
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
            readonly TraitChooseMenu _menu;
            readonly SpriteRenderer _renderer;
            readonly ColorPaletteSpriteElement _paletteElement;

            public RerollButtonDrawer(TraitChooseMenu menu) : base(menu, menu.Transform.Find("Reroll button"))
            {
                _menu = menu;
                _renderer = gameObject.GetComponent<SpriteRenderer>();
                _paletteElement = gameObject.GetComponent<ColorPaletteSpriteElement>();
                _paletteElement.setColorOnStart = false;
                SetTooltip(() => Translator.GetString("trait_choose_menu_1", _menu._rerollsLeft));
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
            readonly TraitChooseMenu _menu;
            readonly SpriteRenderer _renderer;
            readonly ColorPaletteSpriteElement _paletteElement;

            public DeclineButtonDrawer(TraitChooseMenu menu) : base(menu, menu.Transform.Find("Decline button")) 
            {
                _menu = menu;
                _renderer = transform.GetComponent<SpriteRenderer>();
                _paletteElement = gameObject.GetComponent<ColorPaletteSpriteElement>();
                _paletteElement.setColorOnStart = false;
                SetTooltip(() => Translator.GetString("trait_choose_menu_2"));
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

        public TraitChooseMenu(int choices, int traitsPerChoice, int rerolls) : base(ID, _prefab)
        {
            _choicesLeft = choices;
            _traitsPerChoice = traitsPerChoice;
            _rerollsLeft = rerolls;

            _sleeve = new SleeveToChoose(this, Player.Deck);
            _sleeve.TakeMissingCards(true);

            _traits = Array.Empty<TraitToChoose>();
            _traitsParent = Transform.Find("Traits");
            _headerTextMesh = Transform.Find<TextMeshPro>("Header text");
            _selectedCardText = Transform.Find<TextMeshPro>("Selected card text");
            _rerollButton = new RerollButtonDrawer(this);
            _declineButton = new DeclineButtonDrawer(this);

            _headerTextMesh.text = Translator.GetString("trait_choose_menu_3");
            _arrows = new ArrowsAnim[]
            {
                new(Transform.Find("Arrows 1"), 30, 3.6f, -3.68f),
                new(Transform.Find("Arrows 2"), 50, -3.6f, 3.68f),
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
                await ShowTraits();
            base.OnTransitEnd(isFromThis);
            if (!isFromThis)
                _animInProgress = false;
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
            foreach (TraitToChoose trait in _traits)
            {
                if (trait.Drawer != null)
                    trait.Drawer.ColliderEnabled = value;
            }
            _sleeve.Drawer.ColliderEnabled = value;
            _rerollButton.ColliderEnabled = value && _rerollsLeft > 0;
            _declineButton.ColliderEnabled = value;
        }

        Trait[] GenerateTraitsToChoose()
        {
            if (_traitsPerChoice > MAX_TRAITS)
                throw new Exception($"Traits count must not be greater than {MAX_TRAITS}.");
            if (_traitsPerChoice <= 0)
                throw new Exception("Traits count (amount of cards to choose from) was less or equal to 0.");

            if (_choicesLeft > 0)
            {
                Trait[] genTraits = new Trait[_traitsPerChoice];
                for (int i = 0; i < _traitsPerChoice; i++)
                {
                    string srcTraitId = TraitBrowser.GetTraitRandom().id;
                    Trait genTrait = TraitBrowser.NewTrait(srcTraitId);
                    genTraits[i] = genTrait;
                }
                return genTraits;
            }
            Debug.LogError("There are no card choices left to generate cards for.");
            return null;
        }

        void SelectCard(CardToChoose card)
        {
            _chosenCard = card;
            _selectedCardText.text = Translator.GetString("trait_choose_menu_4", card.Data.name.ToUpper());
            _selectedCardText.DOKill();
            _selectedCardText.alpha = 1;
            _selectedCardText.transform.DOKill();
            _selectedCardText.transform.position = _selectedCardText.transform.position.SetY(SELECTED_CARD_TEXT_Y_UPPER);
            _selectedCardText.transform.DOMoveY(SELECTED_CARD_TEXT_Y_NORMAL, 0.5f).SetEase(Ease.OutQuad).SetTarget(_selectedCardText);
        }
        void DeselectCard()
        {
            _chosenCard?.Deselect(false);
            _chosenCard = null;
            _selectedCardText.DOFade(0, 0.5f);
        }

        async UniTask ConfirmChoice(TableTrait? chosenTrait)
        {
            if (ChoicesLeft <= 0 || _animInProgress)
                return;
            if (chosenTrait != null && _chosenCard == null)
            {
                chosenTrait.Drawer.CreateTextAsSpeech(Translator.GetString("trait_choose_menu_5"), Color.red);
                return;
            }
            if (chosenTrait != null && chosenTrait.Data.tags.HasFlag(TraitTag.Static) && _chosenCard.Traits.Any(chosenTrait.Data.id) != null)
            {
                chosenTrait.Drawer.CreateTextAsSpeech(Translator.GetString("trait_choose_menu_6"), Color.red);
                return;
            }

            _animInProgress = true;
            SetCollider(false);
            if (chosenTrait != null)
            {
                Tween tween = chosenTrait.Drawer.AnimHighlightOutline(1.5f);
                Trait data = chosenTrait.Data;
                if (data.isPassive)
                {
                    await _chosenCard.Traits.Passives.AdjustStacks(data.id, 1, this);
                    _chosenCard.Data.traits.Passives.AdjustStacks(data.id, 1);
                }
                else
                {
                    await _chosenCard.Traits.Actives.AdjustStacks(data.id, 1, this);
                    _chosenCard.Data.traits.Actives.AdjustStacks(data.id, 1);
                }
                await tween.AsyncWaitForCompletion();
            }

            _choicesLeft--;
            DeselectCard();
            await HideTraits();
            if (_choicesLeft <= 0)
            {
                await TransitFromThis();
                return;
            }

            await ShowTraits();
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
            await HideTraits();
            await ShowTraits();
            _animInProgress = false;
            SetCollider(true);
        }
        async UniTask DeclineChoice() => await ConfirmChoice(null);

        public async UniTask ShowTraits()
        {
            if (_traits.Length != 0)
                await HideTraits();

            if (_traitsAreShown) return;
            _traitsAreShown = true;

            Tween lastTween = null;
            Trait[] generatedTraits = GenerateTraitsToChoose();
            if (generatedTraits == null) return;
            _traits = new TraitToChoose[generatedTraits.Length];

            for (int i = 0; i < generatedTraits.Length; i++)
            {
                Trait genTrait = generatedTraits[i];
                TableTrait onTable = genTrait.isPassive ? new TablePassiveTrait((PassiveTrait)genTrait, null, _traitsParent) 
                                                        : new TableActiveTrait((ActiveTrait)genTrait, null, _traitsParent);
                TraitToChoose cardToChoose = new(this, onTable, i);
                _traits[i] = cardToChoose;
                lastTween = cardToChoose.AnimShow();
            }

            await lastTween.AsyncWaitForCompletion();
        }
        public async UniTask HideTraits()
        {
            if (_traits == null)
                throw new InvalidOperationException("There are no cards to hide.");

            if (!_traitsAreShown) return;
            _traitsAreShown = false;

            Tween lastTween = null;
            foreach (TraitToChoose card in _traits)
                lastTween = card.AnimHide();

            _traits = Array.Empty<TraitToChoose>();
            await lastTween.AsyncWaitForCompletion();
        }

        void OnUpdate()
        {
            CardToChoose.OnUpdate();
        }
    }
}
