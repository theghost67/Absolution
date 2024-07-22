using DG.Tweening;
using Game.Palette;
using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий обводку для компонента <see cref="SpriteRenderer"/>.
    /// </summary>
    public sealed class SpriteRendererOutline : System.IDisposable
    {
        static readonly Material _outlinePaletteMaterial;
        static readonly Material _outlineMaterial;

        readonly SpriteRenderer _renderer;
        readonly Material _material;

        Color _color;
        Tween _colorTween;

        static SpriteRendererOutline()
        {
            _outlinePaletteMaterial = Resources.Load<Material>("Materials/Outline lit palette material");
            _outlineMaterial = Resources.Load<Material>("Materials/Outline lit material");
        }
        public SpriteRendererOutline(SpriteRenderer renderer, bool paletteSupport) 
        {
            if (paletteSupport)
            {
                _material = Object.Instantiate(_outlinePaletteMaterial);
                _material.name = $"Outline w/Palette for {renderer.name}";
            }
            else
            {
                _material = Object.Instantiate(_outlineMaterial);
                _material.name = $"Outline for {renderer.name}";
            }

            _material.color = Color.clear;
            _renderer = renderer;
            _renderer.GetComponent<Renderer>().material = _material;
            ColorPalette.AddDependantMaterial(_material);
        }

        public void Dispose()
        {
            _colorTween.Kill();
            ColorPalette.RemoveDependantMaterial(_material);
            Object.Destroy(_material);
        }

        public Color GetColor() => _color;
        public Tween GetColorTween() => _colorTween;

        public void SetColor(Color value)
        {
            _color = value;
            if (_material != null)
                 _material.color = value;
            else _colorTween.Kill();
        }
        public Tween TweenColor(Color value, float duration)
        {
            _colorTween.Kill();
            _colorTween = DOVirtual.Color(_color, value, duration, SetColor);
            _colorTween.SetTarget(_renderer);
            return _colorTween;
        }

        public void TweenColorLerpEndless(Color start, Color end, float startDuration, float lerpDuration)
        {
            _colorTween.Kill();
            void PlayLerpEndlessTween()
            {
                DOVirtual.Color(start, end, lerpDuration, SetColor).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
            }

            if (startDuration > 0)
                _colorTween = TweenColor(start, startDuration).OnComplete(PlayLerpEndlessTween);
            else
            {
                SetColor(start);
                PlayLerpEndlessTween();
            }
        }
    }
}
