using DG.Tweening;
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
        static readonly GameObject _prefab;
        static readonly Vector2 _minSizeDelta;

        static readonly Sprite _traitRarityIcon1Sprite;
        static readonly Sprite _traitRarityIcon2Sprite;
        static readonly Sprite _traitRarityIcon3Sprite;

        public readonly new TableTraitListElement attached;
        public bool enqueueAnims;

        readonly SpriteRenderer _rarityIconRenderer;
        readonly SpriteRenderer _traitIconRenderer;
        readonly TextMeshPro _nameTextMesh;
        readonly TextMeshPro _stacksTextMesh;
        readonly RectTransform _rectTransform;

        Tween _appearTween;
        Tween _adjustTween;
        Tween _disappearTween;
        Tween _scrollTween; // note: not used at the moment

        static TableTraitListElementDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Traits/Trait list element");

            _minSizeDelta = new Vector2(0.46f, 0.105f);

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
            _nameTextMesh = transform.Find<TextMeshPro>("Name");
            _stacksTextMesh = transform.Find<TextMeshPro>("Stacks");
            _rectTransform = _nameTextMesh.GetComponent<RectTransform>();

            enqueueAnims = true;
            BlocksSelection = false;
            ChangePointer = ChangePointerBase();

            RedrawRarityIconAsDefault();
            RedrawTraitIconAsDefault();
            RedrawNameAsDefault();
            RedrawStacks();
            SetColor(GetCooldownColor());
        }

        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            _nameTextMesh.sortingOrder = value;
            _stacksTextMesh.sortingOrder = value;
            _rarityIconRenderer.sortingOrder = value;
            _traitIconRenderer.sortingOrder = value;
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            _rarityIconRenderer.color = _rarityIconRenderer.color.WithAlpha(value);
            _traitIconRenderer.color = _traitIconRenderer.color.WithAlpha(value);
            _nameTextMesh.color = _nameTextMesh.color.WithAlpha(value);
            _stacksTextMesh.color = _stacksTextMesh.color.WithAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            _rarityIconRenderer.color = value;
            _traitIconRenderer.color = value;
            _nameTextMesh.color = value;
            _stacksTextMesh.color = value;
        }

        public void RedrawRarityIconAsDefault()
        {
            Sprite sprite = attached.Trait.Data.rarity switch
            {
                Rarity.None => _traitRarityIcon1Sprite,
                Rarity.Rare => _traitRarityIcon2Sprite,
                Rarity.Epic => _traitRarityIcon3Sprite,
                _ => throw new NotSupportedException(),
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
        public void RedrawStacks()
        {
            RedrawStacks(attached.Stacks);
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
            _nameTextMesh.text = text;

            Transform parent = transform.parent;
            transform.SetParent(Global.Root, worldPositionStays: true); // ignore current parent active state (otherwise this method returns Vector2.zero)
            _nameTextMesh.ForceMeshUpdate(ignoreActiveState: true);

            Vector2 deltaOld = _rectTransform.sizeDelta;
            Vector2 deltaNew = new(deltaOld.x.SelectMax(_minSizeDelta.x), deltaOld.y.SelectMax(_minSizeDelta.y));

            _rectTransform.sizeDelta = deltaNew;
            transform.SetParent(parent, worldPositionStays: true);
        }
        public void RedrawStacks(int stacks)
        {
            if (stacks <= 0)
                 _stacksTextMesh.text = "-";
            else _stacksTextMesh.text = "x" + stacks.ToString();
        }

        public Vector2 GetSizeDelta() => _rectTransform.sizeDelta;
        public Color GetCooldownColor()
        {
            bool hasCooldown = attached.Trait.Storage.turnsDelay > 0;
            return ColorPalette.All[hasCooldown ? 1 : 0].ColorCur;
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
        public Tween AnimScroll(float y)
        {
            float srcPosY = transform.localPosition.y;
            _scrollTween.Kill();
            _scrollTween = DOVirtual.Float(srcPosY, y, TWEEN_DURATION, OnTweenUpdatePosY).SetEase(Ease.OutQuad);
            return _scrollTween;
        }

        protected virtual bool ChangePointerBase() => false;
        protected virtual bool ShakeOnMouseClickLeft() => true;
        protected override bool HandleMouseEventsAfterClick()
        {
            if (attached.Trait.Owner is ITableSleeveCard sleeveCard && (sleeveCard.Sleeve?.Contains(sleeveCard) ?? false))
                 return sleeveCard.Sleeve.Drawer.IsPulledOut;
            else return true;
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            _appearTween.Kill();
            _adjustTween.Kill();
            _disappearTween.Kill();
            _scrollTween.Kill();
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e) 
        {
            base.OnMouseEnterBase(sender, e);
            ITableTrait trait = attached.Trait;
            Menu.WriteDescToCurrent(trait.DescRich());

            bool isPassive = trait.Data.isPassive;
            Color color = ColorPalette.All[isPassive ? 5 : 6].ColorCur;

            _nameTextMesh.text = trait.Data.name.Underlined();
            _stacksTextMesh.color = color;
            _traitIconRenderer.color = color;
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e) 
        {
            base.OnMouseLeaveBase(sender, e);
            ITableTrait trait = attached.Trait;
            if (trait.Owner.Drawer.IsSelected && !trait.Owner.Drawer.Traits.IsSelected)
                Menu.WriteDescToCurrent(trait.Owner.DescRich());

            _nameTextMesh.text = trait.Data.name;
            SetColor(GetCooldownColor());
        }
        protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseClickBase(sender, e);
            if (!e.isLmbDown) return;

            e.handled |= TableEventManager.CanAwaitAnyEvents();
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
            return DOVirtual.Float(_nameTextMesh.color.a, to, TWEEN_DURATION, OnTweenUpdateAlpha).SetEase(Ease.OutQuad);
        }
        Tween AnimStacks(int to)
        {
            if (!int.TryParse(_stacksTextMesh.text[1..], out int from))
                return null;

            if (to == from) return null;
            if (to > from)
                 _stacksTextMesh.color = Color.green;
            else _stacksTextMesh.color = Color.red;

            return DOVirtual.Int(from, to, TWEEN_DURATION, OnTweenUpdateStacks).OnComplete(() => _stacksTextMesh.color = Color);
        }
        Tween AnimStacksScale(Vector3 to)
        {
            return DOVirtual.Vector3(_stacksTextMesh.transform.localScale, to, TWEEN_DURATION, OnTweenUpdateStacksScale);
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
            _stacksTextMesh.transform.localScale = value;
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
