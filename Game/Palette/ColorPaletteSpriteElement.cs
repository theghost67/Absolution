using MyBox;
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
        [SerializeField] int _syncedColorIndex = 0;
        [SerializeField] float _opacity = -1;
        SpriteRenderer _spriteRenderer;

        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.color = GetPaletteColor();
            ColorPalette.OnColorChanged += OnPaletteColorChanged;
        }
        void OnDestroy()
        {
            ColorPalette.OnColorChanged -= OnPaletteColorChanged;
        }

        void OnPaletteColorChanged(int index)
        {
            if (index != _syncedColorIndex) return;
            _spriteRenderer.color = GetPaletteColor();
        }
        Color GetPaletteColor()
        {
            float a = _opacity == -1 ? _spriteRenderer.color.a : _opacity;
            return ColorPalette.GetColor(_syncedColorIndex).WithAlpha(a);
        }
    }
}
