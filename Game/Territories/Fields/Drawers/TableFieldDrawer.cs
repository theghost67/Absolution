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

        static readonly new GameObject _prefab;
        public readonly TableField attached;

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
            _light.color = ColorPalette.GetColor(0);

            OnMouseClickLeft += OnMouseClickLeftBase;
            ColorPalette.OnColorChanged += OnColorPaletteChanged;
        }

        public override void SetCollider(bool value)
        {
            base.SetCollider(value);
            attached.Card?.Drawer?.SetCollider(value);
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
            cardDrawer.SetSortingOrder(GetSortingOrder() + 1);
            _attachTween = cardDrawer.transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutExpo);
            return _attachTween;
        }
        public Tween AnimDetatchCard(TableCardDrawer cardDrawer)
        {
            return _attachTween; // TODO: implement?
        }

        protected virtual void OnColorPaletteChanged(int index)
        {
            if (index == 1)
                _light.color = ColorPalette.GetColor(index);
        }
        protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
        {
            TableFieldDrawer drawer = (TableFieldDrawer)sender;
            drawer.transform.DOAShake();
        }
        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();
            ColorPalette.OnColorChanged -= OnColorPaletteChanged;
        }
    }
}
