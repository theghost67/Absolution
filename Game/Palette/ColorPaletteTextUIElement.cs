using MyBox;
using TMPro;
using UnityEngine;

namespace Game.Palette
{
    /// <summary>
    ///  ласс, представл€ющий элемент графического интерфейса (<see cref="TextMeshProUGUI"/>) с возможностью<br/>
    /// синхронизации цвета с палитрой (этот элемент не должен иметь материал шейдера палитры).
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ColorPaletteTextUIElement : MonoBehaviour
    {
        [SerializeField] int _syncedColorIndex = 0;
        [SerializeField] float _opacity = -1;
        private TextMeshProUGUI _textMesh;

        void Start()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
            _textMesh.color = GetPaletteColor();
            ColorPalette.OnColorChanged += OnPaletteColorChanged;
        }
        void OnDestroy()
        {
            ColorPalette.OnColorChanged -= OnPaletteColorChanged;
        }

        void OnPaletteColorChanged(IPaletteColorInfo info)
        {
            if (info.Index != _syncedColorIndex) return;
            _textMesh.color = GetPaletteColor();
        }
        Color GetPaletteColor()
        {
            float a = _opacity == -1 ? _textMesh.color.a : _opacity;
            return ColorPalette.All[_syncedColorIndex].ColorCur.WithAlpha(a);
        }
    }
}