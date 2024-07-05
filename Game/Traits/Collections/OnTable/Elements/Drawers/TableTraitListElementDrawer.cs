using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using Game.Palette;
using Game.Sleeves;
using GreenOne;
using MyBox;
using System;
using TMPro;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий взаимодействие пользователя с типом <see cref="ITableTraitListElement"/>.
    /// </summary>
    public abstract class TableTraitListElementDrawer : Drawer
    {
        const float TWEEN_DURATION = 0.50f;
        const float ACTIVATION_DUR_APPEAR = 0.50f;
        const float ACTIVATION_DUR_DISPLAY = 1.00f;
        const float ACTIVATION_DUR_DISAPPEAR = 0.50f;

        static readonly GameObject _prefab;
        static readonly GameObject _activationPrefab;
        static readonly Vector2 _minSizeDelta;

        static readonly Sprite _traitRarityIcon1Sprite;
        static readonly Sprite _traitRarityIcon2Sprite;
        static readonly Sprite _traitRarityIcon3Sprite;

        public bool IsActivated => _isActivated;
        public readonly new TableTraitListElement attached;
        public bool enqueueAnims;

        readonly SpriteRenderer _rarityIconRenderer;
        readonly SpriteRenderer _traitIconRenderer;
        readonly TextMeshPro _nameText;
        readonly TextMeshPro _stacksText;
        readonly RectTransform _rectTransform;

        Tween _appearTween;
        Tween _adjustTween;
        Tween _disappearTween;
        Tween _activationTween;
        Tween _scrollTween; // note: not used at the moment

        bool _isActivated;

        static TableTraitListElementDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Traits/Trait list element");
            _activationPrefab = Resources.Load<GameObject>("Prefabs/Traits/Trait activation");
            _minSizeDelta = new Vector2(0.64f, 0.12f);

            _traitRarityIcon1Sprite = Resources.Load<Sprite>("Sprites/Traits/Parts/trait rarity icon 1");
            _traitRarityIcon2Sprite = Resources.Load<Sprite>("Sprites/Traits/Parts/trait rarity icon 2");
            _traitRarityIcon3Sprite = Resources.Load<Sprite>("Sprites/Traits/Parts/trait rarity icon 3");
        }
        public TableTraitListElementDrawer(TableTraitListElement element, Transform parent) : base(element, _prefab, parent)
        {
            attached = element;
            gameObject.name = element.Trait.Data.id;

            _rarityIconRenderer = gameObject.Find<SpriteRenderer>("Rarity icon");
            _traitIconRenderer = gameObject.Find<SpriteRenderer>("Trait icon");
            _nameText = transform.Find<TextMeshPro>("Name");
            _stacksText = transform.Find<TextMeshPro>("Stacks");
            _rectTransform = _nameText.GetComponent<RectTransform>();

            enqueueAnims = true;
            BlocksSelection = false;
            ChangePointer = ChangePointerBase();
            OnMouseEnter += OnMouseEnterBase;
            OnMouseLeave += OnMouseLeaveBase;
            OnMouseClickLeft += OnMouseClickLeftBase;

            RedrawRarityIconAsDefault();
            RedrawTraitIconAsDefault();
            RedrawNameAsDefault();
            RedrawStacksAsDefault();
            SetColor(GetCooldownColor());
        }

        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            _nameText.sortingOrder = value;
            _stacksText.sortingOrder = value;
            _rarityIconRenderer.sortingOrder = value;
            _traitIconRenderer.sortingOrder = value;
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            _rarityIconRenderer.color = _rarityIconRenderer.color.WithAlpha(value);
            _traitIconRenderer.color = _traitIconRenderer.color.WithAlpha(value);
            _nameText.color = _nameText.color.WithAlpha(value);
            _stacksText.color = _stacksText.color.WithAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            _rarityIconRenderer.color = value;
            _traitIconRenderer.color = value;
            _nameText.color = value;
            _stacksText.color = value;
        }

        public void RedrawRarityIconAsDefault()
        {
            Sprite sprite = attached.Trait.Data.rarity switch
            {
                Rarity.None => _traitRarityIcon1Sprite,
                Rarity.Rare => _traitRarityIcon2Sprite,
                Rarity.Epic => _traitRarityIcon3Sprite,
                _ => throw new System.NotSupportedException(),
            };
            RedrawRarityIcon(sprite);
        }
        public void RedrawTraitIconAsDefault()
        {
            RedrawTraitIcon(Resources.Load<Sprite>(attached.Trait.Data.spritePath));
        }
        public void RedrawNameAsDefault()
        {
            RedrawName(attached.Trait.Data.name);
        }
        public void RedrawStacksAsDefault()
        {
            RedrawStacks(attached.Trait.Owner.Data.traits[attached.Trait.Data.id]?.Stacks ?? 0);
        }

        public void RedrawRarityIcon(Sprite sprite)
        {
            _rarityIconRenderer.sprite = sprite;
        }
        public void RedrawTraitIcon(Sprite sprite)
        {
            _traitIconRenderer.sprite = sprite;
        }
        public void RedrawName(string text)
        {
            _nameText.text = text;
        }
        public void RedrawStacks(int stacks)
        {
            if (stacks <= 0)
                 _stacksText.text = "-";
            else _stacksText.text = "x" + stacks.ToString();
        }

        public Vector2 GetSizeDelta()
        {
            Transform parent = transform.parent;
            transform.SetParent(Global.Root, worldPositionStays: true); // ignore current parent active state (otherwise this method returns Vector2.zero)
            _nameText.ForceMeshUpdate(ignoreActiveState: true);
            _rectTransform.sizeDelta = _nameText.textBounds.size;
            transform.SetParent(parent, worldPositionStays: true);

            Vector2 sizeDelta = _rectTransform.sizeDelta;
            return new Vector2(sizeDelta.x.SelectMax(_minSizeDelta.x), sizeDelta.y.SelectMax(_minSizeDelta.y));
        }
        public Color GetCooldownColor()
        {
            bool hasCooldown = attached.Trait.Storage.turnsDelay > 0;
            return ColorPalette.GetColor(hasCooldown ? 1 : 0);
        }

        public Tween AnimAppear()
        {
            SetCollider(false);
            OnTweenUpdatePosX(-0.25f);
            SetAlpha(1f);

            _appearTween.Kill();
            _appearTween = AnimPosX(0);
            _appearTween.OnComplete(() => SetCollider(true));
            return _appearTween;
        }
        public Tween AnimAdjust(int toStacks)
        {
            OnTweenUpdateStacksScale(Vector3.one * 1.3f);
            Sequence seq = DOTween.Sequence();
            seq.Append(AnimStacks(toStacks));
            seq.Append(AnimStacksScale(Vector3.one));

            _adjustTween.Kill();
            _adjustTween = seq;
            return seq.Play();
        }
        public Tween AnimDisappear()
        {
            SetCollider(false);
            AnimAlpha(0);

            _disappearTween.Kill();
            _disappearTween = AnimPosX(0.25f);
            return _disappearTween;
        }
        public Tween AnimActivation()
        {
            _activationTween.Kill(complete: true);

            TableTraitListSet set = attached.List.Set;
            TableTraitListSetDrawer setDrawer = set.Drawer;
            TableFieldCardDrawer cardDrawer = set.Owner.Drawer;

            if (setDrawer == null)
                return null;

            _isActivated = true;
            bool hadVisibleBg = cardDrawer.BgIsVisible;
            setDrawer.HideStoredElementsInstantly();
            cardDrawer.ShowBgInstantly();

            Trait data = attached.Trait.Data;
            GameObject prefab = GameObject.Instantiate(_activationPrefab, cardDrawer.transform);
            Transform prefabTransform = prefab.transform;
            TextMeshPro prefabTextMesh = prefabTransform.Find<TextMeshPro>("Text");
            SpriteRenderer prefabRenderer = prefabTransform.Find<SpriteRenderer>("Icon");

            prefabRenderer.sprite = Resources.Load<Sprite>(data.spritePath);
            prefabTextMesh.text = data.name;

            Vector3 scale1 = Vector3.one * 2.00f;
            Vector3 scale2 = Vector3.one * 1.00f;
            Vector3 scale3 = Vector3.one * 0.75f;

            Color color1 = ColorPalette.GetColor(0);
            Color color2 = ColorPalette.GetColor(data.isPassive ? 5 : 6);
            Color color3 = ColorPalette.GetColor(1).WithAlpha(0);

            prefabTransform.localScale = scale1;
            prefabRenderer.color = color1;
            prefabTextMesh.color = color1;
            attached.Trait.Owner.Drawer.HighlightOutline(data.isPassive);

            Tween textTween1 = prefabTextMesh.DOColor(color2, ACTIVATION_DUR_APPEAR).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween textTween2 = prefabTextMesh.DOColor(color3, ACTIVATION_DUR_DISAPPEAR).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween spriteTween1 = prefabRenderer.DOColor(color2, ACTIVATION_DUR_APPEAR).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween spriteTween2 = prefabRenderer.DOColor(color3, ACTIVATION_DUR_DISAPPEAR).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween scaleTween1 = prefabTransform.DOScale(scale2, ACTIVATION_DUR_APPEAR).Pause().SetTarget(prefab).SetEase(Ease.OutCubic);
            Tween scaleTween2 = prefabTransform.DOScale(scale3, ACTIVATION_DUR_DISAPPEAR).Pause().SetTarget(prefab).SetEase(Ease.OutCubic);

            Sequence seq = DOTween.Sequence(prefab);
            seq.AppendCallback(() =>
            {
                scaleTween1.Play();
                textTween1.Play();
                spriteTween1.Play();
            });
            seq.AppendInterval(ACTIVATION_DUR_DISPLAY);
            seq.AppendCallback(() =>
            {
                scaleTween2.Play();
                textTween2.Play();
                spriteTween2.Play();
            });
            seq.AppendInterval(ACTIVATION_DUR_DISAPPEAR);
            seq.OnComplete(() =>
            {
                prefab.Destroy();
                if (cardDrawer.IsSelected || setDrawer.IsSelected)
                    setDrawer.ShowStoredElements();
                else if (!hadVisibleBg) cardDrawer.HideBg();
                _isActivated = false;
            });

            _activationTween = seq;
            return seq.Play();
        }
        public Tween AnimScroll(float y)
        {
            float srcPosY = transform.localPosition.y;
            _scrollTween.Kill();
            _scrollTween = DOVirtual.Float(srcPosY, y, TWEEN_DURATION, OnTweenUpdatePosY).SetEase(Ease.OutQuad);
            return _scrollTween;
        }

        protected virtual bool ChangePointerBase() => false;
        protected virtual bool ShakeOnMouseClickLeft() => true;
        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            _appearTween.Kill();
            _adjustTween.Kill();
            _disappearTween.Kill();
            _activationTween.Kill();
            _scrollTween.Kill();
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e) 
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;

            ITableTrait trait = attached.Trait;
            Menu.WriteDescToCurrent(trait.DescRich());

            bool isPassive = trait.Data.isPassive;
            Color color = ColorPalette.GetColor(isPassive ? 5 : 6);

            _nameText.text = trait.Data.name.Underlined();
            _stacksText.color = color;
            _traitIconRenderer.color = color;
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e) 
        {
            base.OnMouseLeaveBase(sender, e);
            if (e.handled) return;

            ITableTrait trait = attached.Trait;
            if (trait.Owner.Drawer.IsSelected && !trait.Owner.Drawer.Traits.elements.IsAnySelected)
                Menu.WriteDescToCurrent(trait.Owner.DescRich());

            _nameText.text = trait.Data.name;
            SetColor(GetCooldownColor());
        }
        protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseClickLeftBase(sender, e);
            if (e.handled) return;

            if (attached.Trait.Owner is ITableSleeveCard sleeveCard)
                e.handled = sleeveCard.IsInMove || sleeveCard.Sleeve.Drawer.IsInMove || (sleeveCard.IsPulledOut && !sleeveCard.Sleeve.Drawer.IsPulledOut);

            if (e.handled) return;
            if (ShakeOnMouseClickLeft())
                transform.DOAShake();
        }
        protected override void OnEnableBase(object sender, EventArgs e)
        {
            base.OnEnableBase(sender, e);
            SetColor(GetCooldownColor());
        }

        Tween AnimAlpha(float to)
        {
            return DOVirtual.Float(_nameText.color.a, to, TWEEN_DURATION, OnTweenUpdateAlpha).SetEase(Ease.OutQuad);
        }
        Tween AnimStacks(int to)
        {
            if (!int.TryParse(_stacksText.text[1..], out int from))
                return null;

            if (to == from) return null;
            if (to > from)
                 _stacksText.color = Color.green;
            else _stacksText.color = Color.red;

            return DOVirtual.Int(from, to, TWEEN_DURATION, OnTweenUpdateStacks).OnComplete(() => _stacksText.color = Color);
        }
        Tween AnimStacksScale(Vector3 to)
        {
            return DOVirtual.Vector3(_stacksText.transform.localScale, to, TWEEN_DURATION, OnTweenUpdateStacksScale);
        }
        Tween AnimPosX(float to)
        {
            float srcPosX = transform.localPosition.x;
            return DOVirtual.Float(srcPosX, to, TWEEN_DURATION, OnTweenUpdatePosX).SetEase(Ease.OutQuad);
        }

        void OnTweenUpdateAlpha(float value)
        {
            SetAlpha(value);
        }
        void OnTweenUpdateStacks(int value)
        {
            RedrawStacks(value);
        }
        void OnTweenUpdateStacksScale(Vector3 value)
        {
            _stacksText.transform.localScale = value;
        }
        void OnTweenUpdatePosX(float value)
        {
            transform.localPosition = transform.localPosition.SetX(value);
        }
        void OnTweenUpdatePosY(float value)
        {
            transform.localPosition = transform.localPosition.SetY(value);
        }
    }
}
