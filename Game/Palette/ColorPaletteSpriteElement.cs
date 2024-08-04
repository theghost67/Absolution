using UnityEngine;

namespace Game.Palette
{
    /// <summary>
    ///  ласс, представл€ющий элемент графического интерфейса (<see cref="SpriteRenderer"/>) с возможностью<br/>
    /// синхронизации цвета с палитрой (этот элемент не должен иметь материал шейдера палитры).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ColorPaletteSpriteElement : MonoBehaviour
    {
        public Color SyncedColor => GetPaletteColor();
        public int SyncedColorIndex => _syncedColorIndex;
        public float Opacity => _opacity;
        public float Multiplier => _multiplier;

        public bool setColorOnStart = true;

        [SerializeField] int _syncedColorIndex = 0;
        [SerializeField] float _opacity = -1;
        [SerializeField] float _multiplier = 1;
        SpriteRenderer _spriteRenderer;

        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (setColorOnStart)
                _spriteRenderer.color = GetPaletteColor();
            ColorPalette.OnColorChanged += OnPaletteColorChanged;
        }
        void OnDestroy()
        {
            ColorPalette.OnColorChanged -= OnPaletteColorChanged;
        }

        void OnPaletteColorChanged(IPaletteColorInfo info)
        {
            if (info.Index != _syncedColorIndex) return;
            _spriteRenderer.color = GetPaletteColor();
        }
        Color GetPaletteColor()
        {
            float a = _opacity == -1 ? _spriteRenderer.color.a : _opacity;
            Color color = ColorPalette.All[_syncedColorIndex].ColorCur * _multiplier;
            color.a = a;
            return color;
        }
    }
}
