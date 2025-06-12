using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using GreenOne;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableField"/>.
    /// </summary>
    public class TableFieldDrawer : SelectableDrawer
    {
        public const int WIDTH = TableCardDrawer.WIDTH;
        public const int HEIGHT = TableCardDrawer.HEIGHT;

        public readonly new TableField attached;
        static readonly GameObject _prefab;

        protected override SpriteRenderer SelectableRenderer => _spriteRenderer;
        readonly SpriteRenderer _spriteRenderer;

        bool _isHighlighted;
        Light2D _light;
        Tween _lightTween;
        Tween _attachTween;

        static TableFieldDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Territories/Field");
        }
        public TableFieldDrawer(TableField field, Transform parent) : base(field, _prefab, parent) 
        {
            attached = field;

            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _light = transform.Find<Light2D>("Light");
            _light.color = ColorPalette.C1.ColorCur;

            OnMouseClick += OnMouseClickBase;
            ColorPalette.OnColorChanged += OnColorPaletteChanged;
        }

        public void SetHighlight(bool value)
        {
            if (_isHighlighted != value)
                _isHighlighted = value;
            else return;

            if (_isHighlighted)
            {
                _light.enabled = true;
                _lightTween = DOVirtual.Float(0, 5, 1, v => _light.intensity = v);
                _lightTween.SetEase(Ease.InOutCubic);
            }
            else
            {
                _light.enabled = false;
                _light.intensity = 0;
                _lightTween.Kill();
            }
        }
        public void FlipY()
        {
            _spriteRenderer.flipY = !_spriteRenderer.flipY;
        }

        public Tween AnimAttachCard(TableCardDrawer cardDrawer)
        {
            if (cardDrawer == null) return _attachTween;
            cardDrawer.transform.SetParent(transform, worldPositionStays: true);
            cardDrawer.SortingOrder = attached.pos.x * 20;
            _attachTween = cardDrawer.transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutExpo);
            return _attachTween;
        }
        public Tween AnimDetatchCard(TableCardDrawer cardDrawer)
        {
            cardDrawer.SortingOrder = 120;
            return _attachTween;
        }

        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            TableFieldCardDrawer drawer = attached.Card?.Drawer;
            if (drawer != null)
                drawer.ColliderEnabled = value;
        }
        protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseClickBase(sender, e);
            if (!e.isLmbDown) return;

            TableFieldDrawer drawer = (TableFieldDrawer)sender;
            drawer.transform.DOAShake();
        }
        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();
            ColorPalette.OnColorChanged -= OnColorPaletteChanged;
        }

        void OnColorPaletteChanged(IPaletteColorInfo info)
        {
            if (info.Index == 1)
                _light.color = info.ColorCur;
        }
    }
}
