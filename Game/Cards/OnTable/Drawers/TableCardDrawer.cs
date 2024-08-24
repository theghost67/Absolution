﻿using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Menus;
using Game.Palette;
using GreenOne;
using MyBox;
using TMPro;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс, представляющий взаимодействие пользователя с типом <see cref="TableCard"/>.
    /// </summary>
    public abstract class TableCardDrawer : SelectableDrawer
    {
        public const int WIDTH = 74;
        public const int HEIGHT = 112;
        const float BG_ALPHA_MAX = 0.8f;

        public bool IsFlipped => _isFlipped;
        public bool BgIsVisible => _bgIsVisible;
        public SpriteRendererOutline Outline => _outline;
        protected override SpriteRenderer SelectableRenderer => _spriteRenderer;

        protected static readonly Sprite uGoldIconSprite;     // u == designed for upper icons
        protected static readonly Sprite uEtherIconSprite;
        protected static readonly Sprite uMoxieIconSprite;
        protected static readonly Sprite lHealthIconSprite;   // l == designed for lower icons
        protected static readonly Sprite lStrengthIconSprite;

        public readonly new TableCard attached;
        public readonly TableCardUpperIconDrawer priceIcon;
        public readonly TableCardUpperIconDrawer moxieIcon;
        public readonly TableCardLowerIconDrawer healthIcon;
        public readonly TableCardLowerIconDrawer strengthIcon;

        static readonly GameObject _prefab;
        static readonly Sprite _cardRarity1Sprite;
        static readonly Sprite _cardRarity2Sprite;
        static readonly Sprite _cardRarity3Sprite;

        static Color _outlineDimPassiveColor;
        static Color _outlineDimActiveColor;

        readonly SpriteRenderer _portraitRenderer;
        readonly SpriteRenderer _spriteRenderer;
        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _subheaderTextMesh;

        readonly SpriteRenderer _bgRenderer;
        readonly SpriteRendererOutline _outline;

        bool _isFlipped;
        bool _bgIsVisible;
        Tween _bgTween;
        Tween _headerTween;

        protected enum OutlineType
        {
            None,
            Passive,
            Active,
            Both
        }

        static TableCardDrawer()
        {
            uGoldIconSprite = Resources.Load<Sprite>("Sprites/Cards/Parts/card gold icon");
            uEtherIconSprite = Resources.Load<Sprite>("Sprites/Cards/Parts/card ether icon");
            uMoxieIconSprite = Resources.Load<Sprite>("Sprites/Cards/Parts/card moxie icon");
            lHealthIconSprite = Resources.Load<Sprite>("Sprites/Cards/Parts/card health icon");
            lStrengthIconSprite = Resources.Load<Sprite>("Sprites/Cards/Parts/card strength icon");

            _prefab = Resources.Load<GameObject>("Prefabs/Cards/Card");
            _cardRarity1Sprite = Resources.Load<Sprite>("Sprites/Cards/card rarity 1");
            _cardRarity2Sprite = Resources.Load<Sprite>("Sprites/Cards/card rarity 2");
            _cardRarity3Sprite = Resources.Load<Sprite>("Sprites/Cards/card rarity 3");

            OnPaletteColorChanged_Static(ColorPalette.CP);
            OnPaletteColorChanged_Static(ColorPalette.CA);
            ColorPalette.OnColorChanged += OnPaletteColorChanged_Static;
        }
        public TableCardDrawer(TableCard card, Transform parent) : base(card, _prefab, parent)
        {
            attached = card;
            gameObject.name = attached.Data.id;

            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _headerTextMesh = transform.Find<TextMeshPro>("Header");
            _subheaderTextMesh = transform.Find<TextMeshPro>("Subheader");
            _portraitRenderer = gameObject.Find<SpriteRenderer>("Portrait");
            _bgRenderer = transform.Find<SpriteRenderer>("BG");
            _outline = new SpriteRendererOutline(_spriteRenderer, paletteSupport: true);

            priceIcon = new TableCardUpperIconDrawer(this, () => UpperLeftIconDisplayValue(), transform.Find("Upper left icon"));
            priceIcon.SetTooltip(UpperLeftIconTooltip);

            moxieIcon = new TableCardUpperIconDrawer(this, () => UpperRightIconDisplayValue(), transform.Find("Upper right icon"));
            moxieIcon.SetTooltip(UpperRightIconTooltip);

            healthIcon = new TableCardLowerIconDrawer(this, () => LowerLeftIconDisplayValue(), transform.Find("Lower left icon"));
            healthIcon.SetTooltip(LowerLeftIconTooltip);

            strengthIcon = new TableCardLowerIconDrawer(this, () => LowerRightIconDisplayValue(), transform.Find("Lower right icon"));
            strengthIcon.SetTooltip(LowerRightIconTooltip);

            ColorPalette.OnColorChanged += OnPaletteColorChanged;
            RedrawSpriteAsDefault();
            RedrawPortraitAsDefault();
            RedrawHeaderAsDefault();
            RedrawHeaderColor(Color.black);
        }

        public void RedrawSpriteAsDefault()
        {
            Sprite sprite = attached.Data.rarity switch
            {
                Rarity.None => _cardRarity1Sprite,
                Rarity.Rare => _cardRarity2Sprite,
                Rarity.Epic => _cardRarity3Sprite,
                _ => throw new System.NotSupportedException(),
            };
            RedrawSprite(sprite);
        }
        public void RedrawPortraitAsDefault()
        {
            RedrawPortrait(Resources.Load<Sprite>(attached.Data.spritePath));
        }
        public void RedrawHeaderAsDefault()
        {
            RedrawHeader(attached.Data.name);
        }

        public void RedrawSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }
        public void RedrawPortrait(Sprite sprite)
        {
            _portraitRenderer.sprite = sprite;
        }
        public void RedrawHeader(string text)
        {
            _headerTween?.Kill();
            _headerTextMesh.text = text;
        }
        public void RedrawSubheader(string text)
        {
            _subheaderTextMesh.text = text;
        }
        public void RedrawHeaderColor(Color color)
        {
            _headerTextMesh.color = color;
        }
        public void RedrawSubheaderColor(Color color)
        {
            _subheaderTextMesh.color = color;
        }

        public void ShowBg()
        {
            if (_bgIsVisible) return;
            _bgIsVisible = true;
            _bgTween.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(BG_ALPHA_MAX), 0.25f);
        }
        public void ShowBgInstantly()
        {
            if (_bgIsVisible) return;
            _bgIsVisible = true;
            _bgTween.Kill();
            _bgRenderer.color = Color.white.WithAlpha(BG_ALPHA_MAX);
        }
        public void HideBg()
        {
            if (!_bgIsVisible) return;
            _bgIsVisible = false;
            _bgTween.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(0f), 0.25f);
        }
        public void HideBgInstantly()
        {
            if (!_bgIsVisible) return;
            _bgIsVisible = false;
            _bgTween.Kill();
            _bgRenderer.color = Color.white.WithAlpha(0f);
        }
        public void FlipY()
        {
            _isFlipped = !_isFlipped;
            transform.Rotate(Vector3.forward * 180); 
        }

        public async UniTask RedrawHeaderTypingWithReset(params string[] texts)
        {
            await RedrawHeaderTypingBase(resetOnFinish: true, texts);
        }
        public async UniTask RedrawHeaderTyping(params string[] texts)
        {
            await RedrawHeaderTypingBase(resetOnFinish: false, texts);
        }

        public Tween AnimHighlightOutline(float duration)
        {
            return AnimHighlightOutline(duration, Outline.ColorDefault);
        }
        public Tween AnimHighlightOutline(float duration, Color color)
        {
            float factor = Mathf.Pow(2, 6);
            color.r *= factor;
            color.g *= factor;
            color.b *= factor;

            void OnUpdate(float v)
            {
                Color c = Color.Lerp(color, Outline.ColorDefault, v);
                Outline.SetColorCurrent(c);
            }
            return Outline.TweenColorCurrent(OnUpdate, duration).SetEase(Ease.OutQuad);
        }
        public Tween AnimExplosion()
        {
            return _spriteRenderer.DOAExplosion();
        }

        protected void RedrawIcons()
        {
            // TODO: add 'iconSprite' to CardCurrency
            if (attached.Data.price.currency.id == "gold")
                priceIcon.RedrawSprite(uGoldIconSprite);
            else priceIcon.RedrawSprite(uEtherIconSprite);

            priceIcon.RedrawValue();
            moxieIcon.RedrawValue();
            healthIcon.RedrawValue();
            strengthIcon.RedrawValue();
        }
        protected void RedrawOutline()
        {
            const float DURATION = 1.5f;
            switch (GetOutlineType())
            {
                case OutlineType.Passive: Outline.TweenColorDefault(_outlineDimPassiveColor, DURATION); break;
                case OutlineType.Active: Outline.TweenColorDefault(_outlineDimActiveColor, DURATION); break;
                case OutlineType.Both: Outline.TweenColorDefaultLoop(_outlineDimPassiveColor, _outlineDimActiveColor, DURATION); break;
                default: Outline.TweenColorDefault(ColorPalette.C1.ColorCur, DURATION); break;
            }
        }
        protected void RedrawOutlineInstantly()
        {
            const float DURATION = 1.5f;
            switch (GetOutlineType())
            {
                case OutlineType.Passive: Outline.SetColorDefault(_outlineDimPassiveColor); break;
                case OutlineType.Active: Outline.SetColorDefault(_outlineDimActiveColor); break;
                case OutlineType.Both:
                    Outline.SetColorDefault(_outlineDimPassiveColor);
                    Outline.TweenColorDefaultLoop(_outlineDimPassiveColor, _outlineDimActiveColor, DURATION);
                    break;
                default: Outline.SetColorDefault(ColorPalette.C1.ColorCur); break;
            }
        }
        protected virtual OutlineType GetOutlineType()
        {
            return OutlineType.None;
        }

        protected virtual int UpperLeftIconDisplayValue()
        {
            return attached.Price;
        }
        protected virtual int UpperRightIconDisplayValue() 
        {
            return 0;
        }
        protected virtual int LowerLeftIconDisplayValue() 
        {
            return 0;
        }
        protected virtual int LowerRightIconDisplayValue() 
        {
            return 0;
        }

        protected virtual string UpperLeftIconTooltip() => "";
        protected virtual string UpperRightIconTooltip() => "";
        protected virtual string LowerLeftIconTooltip() => "";
        protected virtual string LowerRightIconTooltip() => "";

        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            int overlapOrder1 = value + 1;
            int overlapOrder2 = value + 2;

            _spriteRenderer.sortingOrder = value;
            _portraitRenderer.sortingOrder = overlapOrder1;
            _headerTextMesh.sortingOrder = overlapOrder1;
            _subheaderTextMesh.sortingOrder = overlapOrder1;
            _bgRenderer.sortingOrder = overlapOrder2;

            priceIcon.SortingOrder = overlapOrder2;
            moxieIcon.SortingOrder = overlapOrder2;
            healthIcon.SortingOrder = overlapOrder2;
            strengthIcon.SortingOrder = overlapOrder2;
        }
        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);

            priceIcon.ColliderEnabled = value;
            moxieIcon.ColliderEnabled = value;
            healthIcon.ColliderEnabled = value;
            strengthIcon.ColliderEnabled = value;
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);

            if (_bgRenderer.color.a != 0)
                _bgRenderer.color = value * BG_ALPHA_MAX;

            priceIcon.Color = value;
            moxieIcon.Color = value;
            healthIcon.Color = value;
            strengthIcon.Color = value;

            _portraitRenderer.color = value;
            _spriteRenderer.color = value;
            _headerTextMesh.color = value;
            _subheaderTextMesh.color = value;
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;

            string desc = attached.DescDynamicWithLinks(out string[] descLinksTexts);
            Menu.WriteDescToCurrent(desc);
            Tooltip.ShowLinks(descLinksTexts);
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            if (e.handled) return;

            Menu.WriteDescToCurrent("");
            Tooltip.Hide();
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            _outline.Dispose();
            _headerTween.Kill();
            ColorPalette.OnColorChanged -= OnPaletteColorChanged;

            priceIcon.TryDestroyInstantly();
            moxieIcon.TryDestroyInstantly();
            healthIcon.TryDestroyInstantly();
            strengthIcon.TryDestroyInstantly();
        }
        protected override UniTask DestroyAnimated()
        {
            base.DestroyAnimated();

            priceIcon.TryDestroyAnimated();
            moxieIcon.TryDestroyAnimated();
            healthIcon.TryDestroyAnimated();
            strengthIcon.TryDestroyAnimated();

            return UniTask.CompletedTask;
        }
        protected override bool CanBeSelected()
        {
            return base.CanBeSelected() && !Sleeves.ITableSleeveCard.IsHoldingAnyCard;
        }

        static void OnPaletteColorChanged_Static(IPaletteColorInfo info)
        {
            Color color = info.ColorCur;
            float grayscale = color.grayscale;
            if (info == ColorPalette.CP)
            {
                color = ((1 - grayscale) * Color.white + color * grayscale).WithAlpha(1);
                _outlineDimPassiveColor = color;
            }
            else if (info == ColorPalette.CA)
            {
                color = ((1 - grayscale) * Color.white + color * grayscale).WithAlpha(1);
                _outlineDimActiveColor = color;
            }
        }
        void OnPaletteColorChanged(IPaletteColorInfo info)
        {
            RedrawOutlineInstantly();
        }
        async UniTask RedrawHeaderTypingBase(bool resetOnFinish, params string[] texts)
        {
            string originText = _headerTextMesh.text;
            _headerTween?.Kill();

            foreach (string text in texts)
            {
                _headerTween = _headerTextMesh.DOATextTyping(text, text.Length * 0.05f, clearText: true);
                await _headerTween.AsyncWaitForCompletion();
                await UniTask.Delay(1000);
            }

            if (resetOnFinish)
            {
                await UniTask.Delay(1000);
                _headerTextMesh.text = originText;
            }
        }
    }
}
