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
        public const int COLORS = 7; // 5 main colors, 2 additional (for traits)
        public const int PASSIVE_INDEX = 5;
        public const int ACTIVE_INDEX = 6;

        public static event Action OnPaletteChanged;
        public static event Action<int> OnColorChanged;

        public static IColorInfo Passive => GetColorInfo(PASSIVE_INDEX);
        public static IColorInfo Active => GetColorInfo(ACTIVE_INDEX);

        static Material _instanceMaterial;
        static ColorInfo[] _instanceColors;
        static List<Material> _instanceDependantMaterials;

        [SerializeField] List<Material> _dependantMaterials;
        [SerializeField] Material _paletteMaterial;
        [NonSerialized] ColorInfo[] _colors;

        public class ColorTweenInfo
        {
            public readonly bool hasStartValue;
            public readonly Color startValue;
            public readonly Color endValue;
            public readonly float duration;

            public ColorTweenInfo(Color endValue, float duration)
            {
                hasStartValue = false;
                this.endValue = endValue;
                this.duration = duration;
            }
            public ColorTweenInfo(Color startValue, Color endValue, float duration)
            {
                hasStartValue = true;
                this.startValue = startValue;
                this.endValue = endValue;
                this.duration = duration;
            }
        }
        public interface IColorInfo
        {
            public Color Color { get; }
            public string Hex { get; }
            public Tween Tween { get; }
            public bool TweenIsPlaying { get; }
        }
        class ColorInfo : IColorInfo
        {
            public Color Color => color;
            public string Hex => hex;
            public Tween Tween => tween;
            public bool TweenIsPlaying => tweenIsPlaying;

            public Color color;
            public string hex;
            public Tween tween;
            public bool tweenIsPlaying;

            public ColorInfo(Color color)
            {
                this.color = color;
                this.hex = color.ToHex();
            }
        }

        static ColorPalette() { _instanceDependantMaterials = new List<Material>(); }
        private ColorPalette() { _dependantMaterials = new List<Material>(); }
        private void Awake()
        {
            if (_instanceColors != null)
                throw new Exception($"There should be only one {nameof(ColorPalette)} instance.");

            _colors = new ColorInfo[COLORS];
            for (int i = 0; i < COLORS; i++)
            {
                Color color = _paletteMaterial.GetColor($"_Color{i}");
                foreach (Material dependantMat in _dependantMaterials)
                    dependantMat.SetColor($"_PaletteColor{i}", color);
                _colors[i] = new ColorInfo(color);
            }

            _instanceColors = _colors;
            _instanceMaterial = _paletteMaterial;
            _instanceDependantMaterials = _dependantMaterials;
        }
        private void Update()
        {
            bool isAnyPlaying = false;
            for (int i = 0; i < COLORS; i++)
            {
                if (_colors[i].tweenIsPlaying)
                {
                    isAnyPlaying = true;
                    break;
                }
            }
            if (isAnyPlaying)
                OnPaletteChanged?.Invoke();
        }

        public static void AddDependantMaterial(Material material, bool update = true)
        {
            _instanceDependantMaterials.Add(material);
            if (update) UpdateDependantMaterial(material);
        }
        public static void UpdateDependantMaterial(Material material)
        {
            for (int i = 0; i < COLORS; i++)
                material.SetColor($"_PaletteColor{i}", _instanceColors[i].color);
        }
        public static void RemoveDependantMaterial(Material material)
        {
            _instanceDependantMaterials.Remove(material);
        }

        public static Color GetColor(int index)
        {
            return _instanceColors[index].color;
        }
        public static IColorInfo GetColorInfo(int index)
        {
            return _instanceColors[index];
        }

        public static void SetColor(int index, Color value)
        {
            SetColorInternal(index, value);
            OnColorChanged?.Invoke(index);
            OnPaletteChanged?.Invoke();
        }
        public static void SetPalette(Color[] values)
        {
            int length = values.Length;
            for (int i = 0; i < COLORS && i < length; i++)
            {
                SetColorInternal(i, values[i]);
                OnColorChanged?.Invoke(i);
            }
            OnPaletteChanged?.Invoke();
        }

        public static Tween TweenColor(int index, ColorTweenInfo info)
        {
            if (info.hasStartValue)
                 SetColor(index, info.startValue);

            ColorInfo pair = _instanceColors[index];
            Tween tween = pair.tween;

            tween?.Kill();
            tween = DOVirtual.Color(pair.color, info.endValue, info.duration, c =>
            {
                SetColorInternal(index, c);
                OnColorChanged?.Invoke(index);
            });
            tween.onComplete += () => pair.tweenIsPlaying = false;

            pair.tweenIsPlaying = true;
            pair.tween = tween;
            return tween;
        }
        public static Tween[] TweenPalette(ColorTweenInfo[] infos)
        {
            if (infos.Length != COLORS)
                throw new ArgumentException("Array size must be equal to pallette size.");

            Tween[] tweens = new Tween[COLORS];
            for (int i = 0; i < COLORS; i++)
                tweens[i] = TweenColor(i, infos[i]);
            return tweens;
        }

        static void SetColorInternal(int index, Color value)
        {
            _instanceColors[index].color = value;
            _instanceColors[index].hex = value.ToHex();
            _instanceMaterial.SetColor($"_Color{index}", value);

            foreach (Material dependantMat in _instanceDependantMaterials)
                dependantMat.SetColor($"_PaletteColor{index}", value);
        }
    }
}
