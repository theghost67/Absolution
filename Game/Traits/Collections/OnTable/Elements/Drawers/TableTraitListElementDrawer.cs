using DG.Tweening;
using Game.Effects;
using Game.Menus;
using Game.Palette;
using Game.Sleeves;
using GreenOne;
using MyBox;
using System;
using System.Linq;
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
            bool cooldown = attached.Trait.IsOnCooldown();
            return ColorPalette.All[cooldown ? 1 : 0].ColorCur;
        }

        public Tween AnimAppear()
        {
            if (IsDestroyed) return null;
            SetCollider(false);
            OnTweenUpdatePosX(-0.25f);
            Alpha = 1f;

            _appearTween.Kill();
            _appearTween = AnimPosX(0);
            _appearTween.OnComplete(() => SetCollider(true));
            return _appearTween;
        }
        public Tween AnimAdjust(int toStacks)
        {
            if (IsDestroyed) return null;
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
            if (IsDestroyed) return null;
            SetCollider(false);
            AnimAlpha(0);

            _disappearTween.Kill();
            _disappearTween = AnimPosX(0.25f);
            return _disappearTween;
        }

        protected virtual bool ChangePointerBase() => false;
        protected virtual bool ShakeOnMouseClickLeft() => true;
        protected override bool HandleMouseEventsAfterClick()
        {
            if (attached.Trait.Owner is ITableSleeveCard sleeveCard && (sleeveCard.Sleeve?.Contains(sleeveCard) ?? false))
                 return sleeveCard.Sleeve.Drawer.IsPulledOut;
            else return true;
        }
        protected override bool CanBeSelected()
        {
            return base.CanBeSelected() && !ITableSleeveCard.IsHoldingAnyCard && (attached.List.Set.Drawer?.IsSelected ?? false);
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            _appearTween.Kill();
            _adjustTween.Kill();
            _disappearTween.Kill();
        }
        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            _nameTextMesh.sortingOrder = value;
            _stacksTextMesh.sortingOrder = value;
            _rarityIconRenderer.sortingOrder = value;
            _traitIconRenderer.sortingOrder = value;
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            _rarityIconRenderer.color = value;
            _traitIconRenderer.color = value;
            _nameTextMesh.color = value;
            _stacksTextMesh.color = value;
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e) 
        {
            base.OnMouseEnterBase(sender, e);
            ITableTrait trait = attached.Trait;

            string desc = trait.DescDynamicWithLinks(out string[] descLinksTexts);
            Menu.WriteDescToCurrent(desc);
            Tooltip.ShowLinks(descLinksTexts);

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

            if (trait.Owner.Drawer.IsSelected && !trait.Owner.Drawer.Traits.IsAnySelected)
            {
                string desc = trait.Owner.DescDynamicWithLinks(out string[] descLinksTexts);
                Menu.WriteDescToCurrent(desc);
                Tooltip.ShowLinks(descLinksTexts);
            }

            _nameTextMesh.text = trait.Data.name;
            SetColor(GetCooldownColor());
        }
        protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseClickBase(sender, e);
            if (!e.isLmbDown) return;

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
            if (IsDestroyed) return null;
            return DOVirtual.Float(_nameTextMesh.color.a, to, TWEEN_DURATION, OnTweenUpdateAlpha).SetEase(Ease.OutQuad);
        }
        Tween AnimStacks(int to)
        {
            if (IsDestroyed) return null;
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
            if (IsDestroyed) return null;
            return DOVirtual.Vector3(_stacksTextMesh.transform.localScale, to, TWEEN_DURATION, OnTweenUpdateStacksScale);
        }
        Tween AnimPosX(float to)
        {
            if (IsDestroyed) return null;
            float srcPosX = transform.localPosition.x;
            return DOVirtual.Float(srcPosX, to, TWEEN_DURATION, OnTweenUpdatePosX).SetEase(Ease.OutQuad);
        }

        void OnTweenUpdateAlpha(float value)
        {
            Alpha = value;
        }
        void OnTweenUpdateStacks(int value)
        {
            RedrawStacks(value);
        }
        void OnTweenUpdateStacksScale(Vector3 value)
        {
            if (_stacksTextMesh == null) return;
            _stacksTextMesh.transform.localScale = value;
        }
        void OnTweenUpdatePosX(float value)
        {
            if (transform == null) return;
            transform.localPosition = transform.localPosition.SetX(value);
        }
        void OnTweenUpdatePosY(float value)
        {
            if (transform == null) return;
            transform.localPosition = transform.localPosition.SetY(value);
        }
    }
}
