using DG.Tweening;
using Game.Palette;
using GreenOne;
using MyBox;
using TMPro;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableTrait"/>.
    /// </summary>
    public class TableTraitDrawer : Drawer
    {
        public const int WIDTH = 7;
        public const int HEIGHT = 7;

        static readonly GameObject _prefab;

        static Color _outlineDimPassiveColor;
        static Color _outlineDimActiveColor;

        public readonly new TableTrait attached;
        public readonly SpriteRenderer icon;

        readonly Sprite _normalSprite;

        static TableTraitDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Traits/Trait");
            ColorPalette.OnColorChanged += OnPaletteColorChanged_Static;
            OnPaletteColorChanged_Static(ColorPalette.CP);
            OnPaletteColorChanged_Static(ColorPalette.CA);
        }
        public TableTraitDrawer(TableTrait trait, Transform parent) : base(trait, _prefab, parent)
        {
            attached = trait;
            gameObject.name = trait.Data.ToString();

            _normalSprite = Resources.Load<Sprite>(attached.Data.spritePath);

            icon = transform.GetComponent<SpriteRenderer>();
            icon.sprite = _normalSprite;

            ChangePointer = ChangePointerBase();
            OnMouseEnter += OnMouseEnterBase;
            OnMouseLeave += OnMouseLeaveBase;
            OnMouseClick += OnMouseClickBase;
            Color = trait.Data.isPassive ? _outlineDimPassiveColor : _outlineDimActiveColor;
        }

        public void RedrawSprite()
        {
            RedrawSprite(_normalSprite);
        }
        public void RedrawSprite(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            icon.color = value;
        }

        public Tween AnimHighlightOutline(float duration)
        {
            return AnimHighlightOutline(duration, Color);
        }
        public Tween AnimHighlightOutline(float duration, Color color)
        {
            Color colorHighlighted = color;
            float factor = Mathf.Pow(2, 6);
            colorHighlighted.r *= factor;
            colorHighlighted.g *= factor;
            colorHighlighted.b *= factor;

            Color = colorHighlighted;
            return this.DOColor(color, duration).SetEase(Ease.OutQuad);
        }

        protected virtual bool ChangePointerBase() => false;
        protected virtual void OnMouseEnterBase() { }
        protected virtual void OnMouseLeaveBase() { }
        protected virtual void OnMouseClickLeftBase() { }

        private static void OnPaletteColorChanged_Static(IPaletteColorInfo info)
        {
            Color color = info.ColorCur;
            float grayscale = color.grayscale * color.grayscale;
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
    }
}
