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

        public Color ColorCurrent => _colorCurrent;
        public Color ColorDefault => _colorDefault;
        public Tween ColorDefaultTween => _colorDefaultTween;
        public Tween ColorCurrentTween => _colorCurrentTween; // if is active, _colorDefaultTween will not update material color

        readonly SpriteRenderer _renderer;
        readonly Material _material;

        Color _colorCurrent;
        Color _colorDefault;
        Tween _colorCurrentTween;
        Tween _colorDefaultTween;

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
            ColorPalette.LinkMaterial(_material);
        }

        public void Dispose()
        {
            _colorDefaultTween.Kill();
            ColorPalette.UnlinkMaterial(_material);
            Object.Destroy(_material);
        }

        public void SetColorCurrent(Color value)
        {
            _colorCurrent = value;
            if (_material != null)
                _material.color = value;
        }
        public void SetColorDefault(Color value)
        {
            _colorDefault = value;
            if (_material != null)
                  _material.color = value;
        }

        public Tween TweenColorCurrent(TweenCallback<float> onUpdate, float duration)
        {
            _colorCurrentTween.Kill();
            _colorCurrentTween = DOVirtual.Float(0, 1, duration, onUpdate); // use ColorDefault in it's onUpdate function
            _colorCurrentTween.SetTarget(_renderer);
            return _colorCurrentTween;
        }
        public Tween TweenColorDefault(Color value, float duration)
        {
            _colorDefaultTween.Kill();
            _colorDefaultTween = DOVirtual.Color(_colorDefault, value, duration, TweenColorDefaultOnUpdate);
            _colorDefaultTween.SetTarget(_renderer);
            return _colorDefaultTween;
        }
        public Tween TweenColorDefaultLoop(Color start, Color end, float duration)
        {
            _colorDefaultTween.Kill();
            void PlayLerpEndlessTween() => DOVirtual.Color(start, end, duration, TweenColorDefaultOnUpdate).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);

            if (duration > 0)
                _colorDefaultTween = TweenColorDefault(start, duration).OnComplete(PlayLerpEndlessTween);
            else throw new System.ArgumentException(nameof(duration));
            return _colorDefaultTween;
        }

        void TweenColorDefaultOnUpdate(Color c)
        {
            if (!_colorCurrentTween.IsActive())
                 SetColorDefault(c);
            else _colorDefault = c;
        }
    }
}
