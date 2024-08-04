using DG.Tweening;
using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Palette
{
    /// <summary>
    /// Класс, представляющий текущую цветовую палитру для графического интерфейса.
    /// </summary>
    public class ColorPalette : MonoBehaviour
    {
        const int COLORS_IN_PALETTE = 7; // 5 main colors, 2 additional (for traits)
        const string LINKED_PROP_PREFIX = "_PaletteColor";

        public static event Action<int> OnPaletteChanged; // int = amount of colors changed since the last update
        public static event Action<IPaletteColorInfo> OnColorChanged;

        // 'c' stands for Color
        public static IPaletteColorInfo C1 => GetInfo(0);
        public static IPaletteColorInfo C2 => GetInfo(1);
        public static IPaletteColorInfo C3 => GetInfo(2);
        public static IPaletteColorInfo C4 => GetInfo(3);
        public static IPaletteColorInfo C5 => GetInfo(4);
        public static IPaletteColorInfo CP => GetInfo(5); // Passive
        public static IPaletteColorInfo CA => GetInfo(6); // Active
        public static IPaletteColorInfo[] All => _instanceColorInfos;

        static ColorInfo[] _instanceColorInfos;
        static List<Material> _instanceLinkedMaterials;
        static int _colorsChangedSinceUpdate;

        [SerializeField] List<Material> _linkedMaterials;
        [SerializeField] Material _shaderMaterial;
        [NonSerialized] ColorInfo[] _colorInfos;

        private class ColorInfo : IEquatable<ColorInfo>, IPaletteColorInfo
        {
            public int Index => _index;
            public string Hex => _colorCur.ToHex();
            public Color ColorDef { get => _colorDef; set => SetColorDef(value); }
            public Color ColorCur { get => _colorCur; set => SetColorCur(value); }
            public Color ColorAll { set { SetColorDef(value); SetColorCur(value); } }

            private readonly int _index;
            private Color _colorDef;
            private Color _colorCur;
            private Tween _colorDefTween;
            private Tween _colorCurTween;

            public ColorInfo(int index, Color defaultValue)
            {
                _index = index;
                SetColorDef(defaultValue);
                SetColorCur(defaultValue);
                UpdateLinkedMaterials();
            }
            public bool Equals(ColorInfo other)
            {
                return _index == other._index;
            }

            public Tween DOColorDef(Func<Color> from, Func<Color> to, float duration)
            {
                SetColorDef(from());
                _colorDefTween?.Kill();
                _colorDefTween = DOVirtual.Float(0, 1, duration, v => SetColorDef(Color.Lerp(from(), to(), v)));
                return _colorDefTween;
            }
            public Tween DOColorCur(Func<Color> from, Func<Color> to, float duration)
            {
                SetColorCur(from());
                _colorCurTween?.Kill();
                _colorCurTween = DOVirtual.Float(0, 1, duration, v => SetColorCur(Color.Lerp(from(), to(), v)));
                return _colorCurTween;
            }

            private void SetColorDef(Color value)
            {
                _colorDef = value;
            }
            private void SetColorCur(Color value)
            {
                _colorCur = value;
                UpdateLinkedMaterials();
                OnColorChanged.Invoke(this);
            }
            private void UpdateLinkedMaterials()
            {
                foreach (Material material in _instanceLinkedMaterials)
                    material.SetColor($"{LINKED_PROP_PREFIX}{_index}", _colorCur);
            }
        }
        private class ColorTweenWrapper
        {
            public Color Color => _color;
            public float Value => _value;
            public Tween Tween => _tween;

            private readonly Func<Color> _from;
            private readonly Func<Color> _to;
            private readonly Action<Color> _onUpdate;
            private readonly float _duration;

            private Color _color;
            private float _value;
            private Tween _tween;

            private ColorTweenWrapper(Func<Color> from, Func<Color> to, float duration, Action<Color> onUpdate)
            {
                _from = from;
                _to = to;
                _duration = duration;
                _onUpdate = onUpdate;
            }
            public static ColorTweenWrapper DOColor(Func<Color> from, Func<Color> to, float duration, Action<Color> onUpdate)
            {
                ColorTweenWrapper wrapper = new(from, to, duration, onUpdate);
                wrapper.CreateTween();
                return wrapper;
            }
            private void CreateTween()
            {
                _tween = DOVirtual.Float(0, 1, _duration, OnTweenUpdate).Play();
            }
            private void OnTweenUpdate(float value)
            {
                _value = value;
                _color = Color.Lerp(_from(), _to(), value);
                _onUpdate(_color);
            }
        }

        static ColorPalette()
        {
            _instanceLinkedMaterials = new List<Material>(); 
        }
        private ColorPalette()
        {
            _linkedMaterials = new List<Material>(); 
        }

        private void Awake()
        {
            if (_instanceColorInfos != null)
                throw new Exception($"There should be only one {nameof(ColorPalette)} instance.");

            if (!_linkedMaterials.Contains(_shaderMaterial))
                throw new Exception($"{nameof(_shaderMaterial)} must be in palette's {nameof(_linkedMaterials)} with valid property names.");

            OnPaletteChanged = null;
            OnColorChanged = i => _colorsChangedSinceUpdate++;

            _colorInfos = new ColorInfo[COLORS_IN_PALETTE];
            for (int i = 0; i < COLORS_IN_PALETTE; i++)
                _colorInfos[i] = new ColorInfo(i, _shaderMaterial.GetColor($"{LINKED_PROP_PREFIX}{i}"));

            _instanceLinkedMaterials = _linkedMaterials;
            _instanceColorInfos = _colorInfos;
        }
        private void Update()
        {
            if (_colorsChangedSinceUpdate == 0) return;
            OnPaletteChanged?.Invoke(_colorsChangedSinceUpdate);
            _colorsChangedSinceUpdate = 0;
        }

        public static void LinkMaterial(Material material)
        {
            _instanceLinkedMaterials.Add(material);
            for (int i = 0; i < COLORS_IN_PALETTE; i++)
                material.SetColor($"{LINKED_PROP_PREFIX}{i}", _instanceColorInfos[i].ColorCur);
        }
        public static void UnlinkMaterial(Material material)
        {
            _instanceLinkedMaterials.Remove(material);
        }
        private static IPaletteColorInfo GetInfo(int index)
        {
            return _instanceColorInfos[index];
        }
    }
}
