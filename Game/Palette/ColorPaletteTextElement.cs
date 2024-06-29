using TMPro;
using UnityEngine;

namespace Game.Palette
{
    /// <summary>
    ///  ласс, представл€ющий элемент графического интерфейса (<see cref="TextMeshPro"/>) с возможностью<br/>
    /// синхронизации цвета с палитрой (этот элемент не должен иметь материал шейдера палитры).
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class ColorPaletteTextElement : MonoBehaviour
    {
        public Color SyncedColor => GetPaletteColor();
        public int SyncedColorIndex => _syncedColorIndex;
        public float Opacity => _opacity;
        public float Multiplier => _multiplier;

        public bool setColorOnStart = true;

        [SerializeField] int _syncedColorIndex = 0;
        [SerializeField] float _opacity = -1;
        [SerializeField] float _multiplier = 1;
        private TextMeshPro _textMesh;

        void Start()
        {
            _textMesh = GetComponent<TextMeshPro>();
            _textMesh.color = GetPaletteColor();
            ColorPalette.OnColorChanged += OnPaletteColorChanged;
        }
        void OnDestroy()
        {
            ColorPalette.OnColorChanged -= OnPaletteColorChanged;
        }

        void OnPaletteColorChanged(int index)
        {
            if (index != _syncedColorIndex) return;
            _textMesh.color = GetPaletteColor();
        }
        Color GetPaletteColor()
        {
            float a = _opacity == -1 ? _textMesh.color.a : _opacity;
            Color color = ColorPalette.GetColor(_syncedColorIndex) * _multiplier;
            color.a = a;
            return color;
        }
    }
}