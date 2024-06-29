using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Menus;
using Game.Palette;
using Game.Traits;
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

        public bool IgnoreFirstMouseEnter { get; set; }
        public bool BgIsVisible => _bgRenderer.color.a != 0;

        public SpriteRendererOutline Outline => _outline;
        protected override SpriteRenderer SelectableRenderer => _spriteRenderer;

        protected static readonly Sprite uGoldIconSprite;     // u == designed for upper icons
        protected static readonly Sprite uEtherIconSprite;
        protected static readonly Sprite uMoxieIconSprite;
        protected static readonly Sprite lHealthIconSprite;   // l == designed for lower icons
        protected static readonly Sprite lStrengthIconSprite;

        public readonly new TableCard attached;
        public readonly TableCardIconDrawer upperLeftIcon;
        public readonly TableCardIconDrawer upperRightIcon;
        public readonly TableCardIconDrawer lowerLeftIcon;
        public readonly TableCardIconDrawer lowerRightIcon;

        static readonly GameObject _prefab;
        static readonly Sprite _cardRarity1Sprite;
        static readonly Sprite _cardRarity2Sprite;
        static readonly Sprite _cardRarity3Sprite;

        static Color _outlineBrightPassiveColor;
        static Color _outlineBrightActiveColor;
        static Color _outlineDimPassiveColor;
        static Color _outlineDimActiveColor;

        readonly SpriteRenderer _portraitRenderer;
        readonly SpriteRenderer _spriteRenderer;
        readonly TextMeshPro _headerTextMesh;
        readonly TextMeshPro _subheaderTextMesh;

        readonly SpriteRenderer _bgRenderer;
        readonly SpriteRendererOutline _outline;

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

            OnPaletteColorChanged_Static(ColorPalette.PASSIVE_INDEX);
            OnPaletteColorChanged_Static(ColorPalette.ACTIVE_INDEX);
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

            upperLeftIcon = new TableCardIconDrawer(this, transform.Find("Upper left icon"), TableCardIconType.Chunks);
            upperLeftIcon.OnMouseEnter += OnUpperLeftIconMouseEnter;
            upperLeftIcon.OnMouseLeave += OnUpperLeftIconMouseLeave;

            upperRightIcon = new TableCardIconDrawer(this, transform.Find("Upper right icon"), TableCardIconType.Chunks);
            upperRightIcon.OnMouseEnter += OnUpperRightIconMouseEnter;
            upperRightIcon.OnMouseLeave += OnUpperRightIconMouseLeave;

            lowerLeftIcon = new TableCardIconDrawer(this, transform.Find("Lower left icon"), TableCardIconType.Texts);
            lowerLeftIcon.OnMouseEnter += OnLowerLeftIconMouseEnter;
            lowerLeftIcon.OnMouseLeave += OnLowerLeftIconMouseLeave;

            lowerRightIcon = new TableCardIconDrawer(this, transform.Find("Lower right icon"), TableCardIconType.Texts);
            lowerRightIcon.OnMouseEnter += OnLowerRightIconMouseEnter;
            lowerRightIcon.OnMouseLeave += OnLowerRightIconMouseLeave;

            ColorPalette.OnColorChanged += OnPaletteColorChanged;
            RedrawSpriteAsDefault();
            RedrawPortraitAsDefault();
            RedrawHeaderAsDefault();
            RedrawHeaderColor(Color.black);
            RedrawPriceAsDefault();
        }

        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            int overlapOrder = value + 1;

            _spriteRenderer.sortingOrder = value;
            _portraitRenderer.sortingOrder = overlapOrder;
            _headerTextMesh.sortingOrder = overlapOrder;
            _subheaderTextMesh.sortingOrder = overlapOrder;
            _bgRenderer.sortingOrder = overlapOrder + 1;

            upperLeftIcon.SetSortingOrder(overlapOrder);
            upperRightIcon.SetSortingOrder(overlapOrder);
            lowerLeftIcon.SetSortingOrder(overlapOrder);
            lowerRightIcon.SetSortingOrder(overlapOrder);
        }
        public override void SetCollider(bool value)
        {
            base.SetCollider(value);

            upperLeftIcon.SetCollider(value);
            upperRightIcon.SetCollider(value);
            lowerLeftIcon.SetCollider(value);
            lowerRightIcon.SetCollider(value);
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);

            if (_bgRenderer.color.a != 0)
                _bgRenderer.SetAlpha(value * BG_ALPHA_MAX);

            upperLeftIcon.SetAlpha(value);
            upperRightIcon.SetAlpha(value);
            lowerLeftIcon.SetAlpha(value);
            lowerRightIcon.SetAlpha(value);

            _portraitRenderer.SetAlpha(value);
            _spriteRenderer.SetAlpha(value);
            _headerTextMesh.SetAlpha(value);
            _subheaderTextMesh.SetAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);

            if (_bgRenderer.color.a != 0)
                _bgRenderer.color = value * BG_ALPHA_MAX;

            upperLeftIcon.SetColor(value);
            upperRightIcon.SetColor(value);
            lowerLeftIcon.SetColor(value);
            lowerRightIcon.SetColor(value);

            _portraitRenderer.color = value;
            _spriteRenderer.color = value;
            _headerTextMesh.color = value;
            _subheaderTextMesh.color = value;
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
        public void RedrawPriceAsDefault()
        {
            RedrawPrice(attached.Data.price.value, attached.Data.price.currency);
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
        public void RedrawPrice(int price, CardCurrency currency)
        {
            upperLeftIcon.RedrawChunks(price);
            upperLeftIcon.RedrawSprite(currency == CardBrowser.GetCurrency("gold") ? uGoldIconSprite : uEtherIconSprite);
        }

        public void ShowBg()
        {
            _bgTween.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(BG_ALPHA_MAX), 0.25f);
        }
        public void HideBg()
        {
            _bgTween.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(0f), 0.25f);
        }

        public void ShowBgInstantly()
        {
            _bgTween.Kill();
            _bgRenderer.color = Color.white.WithAlpha(BG_ALPHA_MAX);
        }
        public void HideBgInstantly()
        {
            _bgTween.Kill();
            _bgRenderer.color = Color.white.WithAlpha(0f);
        }

        public async UniTask RedrawHeaderTypingWithReset(params string[] texts)
        {
            await RedrawHeaderTypingBase(resetOnFinish: true, texts);
        }
        public async UniTask RedrawHeaderTyping(params string[] texts)
        {
            await RedrawHeaderTypingBase(resetOnFinish: false, texts);
        }

        public void HighlightOutline(Color color, float duration = 1)
        {
            Color prevColor = Outline.GetColor();
            float brightF = Mathf.Pow(2, 6);

            color.r *= brightF;
            color.g *= brightF;
            color.b *= brightF;

            if (duration == -1)
                duration = 1;

            Outline.SetColor(color);
            Outline.TweenColor(prevColor, duration).SetEase(Ease.InOutQuad).OnComplete(RedrawOutline);
        }
        public void HighlightOutline(bool asPassive, float duration = 1)
        {
            if (asPassive)
            {
                Outline.SetColor(_outlineBrightPassiveColor);
                Outline.TweenColor(_outlineDimPassiveColor, duration).SetEase(Ease.OutQuad).OnComplete(RedrawOutline);
            }
            else
            {
                Outline.SetColor(_outlineBrightActiveColor);
                Outline.TweenColor(_outlineDimActiveColor, duration).SetEase(Ease.OutQuad).OnComplete(RedrawOutline);
            }
        }

        public Tween AnimExplosion()
        {
            return _spriteRenderer.DOAExplosion();
        }
        protected virtual OutlineType GetOutlineType()
        {
            return OutlineType.None;
        }

        protected void RedrawOutline()
        {
            const float DURATION = 1.5f;
            switch (GetOutlineType())
            {
                case OutlineType.Passive: Outline.TweenColor(_outlineDimPassiveColor, DURATION); break;
                case OutlineType.Active: Outline.TweenColor(_outlineDimActiveColor, DURATION); break;
                case OutlineType.Both: Outline.TweenColorLerpEndless(_outlineDimPassiveColor, _outlineDimActiveColor, DURATION, DURATION); break;

                default:
                    if (Outline.GetColor().a != 0)
                        Outline.TweenColor(Color.clear, DURATION);
                    break;
            }
        }
        protected void RedrawOutlineInstantly()
        {
            const float DURATION = 1.5f;
            switch (GetOutlineType())
            {
                case OutlineType.Passive: Outline.SetColor(_outlineDimPassiveColor); break;
                case OutlineType.Active: Outline.SetColor(_outlineDimActiveColor); break;
                case OutlineType.Both: Outline.TweenColorLerpEndless(_outlineDimPassiveColor, _outlineDimActiveColor, 0, DURATION); break;

                default:
                    if (Outline.GetColor().a != 0)
                        Outline.SetColor(Color.clear);
                    break;
            }
        }

        protected virtual void OnUpperLeftIconMouseEnter(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnUpperRightIconMouseEnter(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnLowerLeftIconMouseEnter(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnLowerRightIconMouseEnter(object sender, DrawerMouseEventArgs e) { }

        protected virtual void OnUpperLeftIconMouseLeave(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnUpperRightIconMouseLeave(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnLowerLeftIconMouseLeave(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnLowerRightIconMouseLeave(object sender, DrawerMouseEventArgs e) { }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;

            if (IgnoreFirstMouseEnter)
            {
                IgnoreFirstMouseEnter = false;
                e.handled = true;
                return;
            }

            Menu.WriteDescToCurrent(attached.DescRich());
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            if (e.handled) return;

            Menu.WriteDescToCurrent("");
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            _outline.Dispose();
            _headerTween.Kill();
            ColorPalette.OnColorChanged -= OnPaletteColorChanged;

            upperLeftIcon.TryDestroyInstantly();
            upperRightIcon.TryDestroyInstantly();
            lowerLeftIcon.TryDestroyInstantly();
            lowerRightIcon.TryDestroyInstantly();
        }
        protected override UniTask DestroyAnimated()
        {
            base.DestroyAnimated();

            upperLeftIcon.TryDestroyAnimated();
            upperRightIcon.TryDestroyAnimated();
            lowerLeftIcon.TryDestroyAnimated();
            lowerRightIcon.TryDestroyAnimated();

            return UniTask.CompletedTask;
        }

        static void OnPaletteColorChanged_Static(int index)
        {
            const int PASSIVE = ColorPalette.PASSIVE_INDEX;
            const int ACTIVE = ColorPalette.ACTIVE_INDEX;
            float brightF = Mathf.Pow(2, 6); // intensity = 6

            if (index == PASSIVE)
            {
                Color color = ColorPalette.GetColor(PASSIVE);
                float grayscale = color.grayscale;
                color = ((1 - grayscale) * Color.white + color * grayscale).WithAlpha(1);

                _outlineBrightPassiveColor = (color * brightF).WithAlpha(1);
                _outlineDimPassiveColor = color;
            }
            else if (index == ACTIVE)
            {
                Color color = ColorPalette.GetColor(ACTIVE);
                float grayscale = color.grayscale;
                color = ((1 - grayscale) * Color.white + color * grayscale).WithAlpha(1);

                _outlineBrightActiveColor = (color * brightF).WithAlpha(1);
                _outlineDimActiveColor = color;
            }
        }
        void OnPaletteColorChanged(int index)
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
