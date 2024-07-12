using TMPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, представляющий методы для отображения подсказок.
    /// </summary>
    public sealed class Tooltip : MonoBehaviour
    {
        public static bool IsShown => _isShown;

        static readonly Vector2 _maxSize = new(6.4f, 3.6f);
        static readonly Vector2 _preferredSize = new(1.5f, 3f);
        static readonly Vector2 _marginSum = new(0.03f * 2, 0.02f * 2);
        static readonly Vector3 _posOffset = new(4f, 4f); // use negative value to flip align side

        static GameObject _gameObject;
        static Transform _transform;
        static SpriteRenderer _renderer;
        static TextMeshPro _text;
        static RectTransform _textRect;
        static bool _isShown;

        public static void Show(string text)
        {
            if (text == "") return;
            bool isNonBrText = text.StartsWith("<nobr>");
            string nonBrText = isNonBrText ? text[(text.IndexOf("<nobr>") + 6)..text.IndexOf("</nobr>")] : "";

            Vector2 textNonBrSize;
            if (isNonBrText)
                textNonBrSize = GetForceUpdatedTextSize(nonBrText, _maxSize);
            else textNonBrSize = Vector2.zero;

            Vector2 optimalSize = GetForceUpdatedTextSize(text, new Vector2(Mathf.Max(_preferredSize.x, textNonBrSize.x), _preferredSize.y));
            Vector2 optimalSizeWithMargin = _marginSum + optimalSize;

            _gameObject.SetActive(true);
            _renderer.size = optimalSizeWithMargin;
            _textRect.sizeDelta = optimalSize;

            _isShown = true;
            MoveToPointer();
        }
        public static void Hide()
        {
            _isShown = false;
            _gameObject.SetActive(false);
        }

        static Vector2 GetForceUpdatedTextSize(string text, Vector2 sizeDelta)
        {
            _textRect.sizeDelta = sizeDelta;
            _text.text = text;
            _text.ForceMeshUpdate(ignoreActiveState: true);

            return _text.textBounds.size;
        }
        static void MoveToPointer()
        {
            if (!_isShown) return;

            Vector3 cameraHalfSize = new(Global.Camera.orthographicSize * Global.ASPECT_RATIO, Global.Camera.orthographicSize);
            Vector3 tooltipHalfSize = _renderer.size * Global.PIXEL_SCALE / 2;

            Vector3 pointerPos = Pointer.Position;
            Vector3 cameraPos = Global.Camera.transform.position;
            Vector3 tooltipPos = new(_posOffset.x < 0 ? pointerPos.x - tooltipHalfSize.x + _posOffset.x : pointerPos.x + tooltipHalfSize.x + _posOffset.x,
                                     _posOffset.y < 0 ? pointerPos.y - tooltipHalfSize.y + _posOffset.y : pointerPos.y + tooltipHalfSize.y + _posOffset.y);

            Vector4 cameraEdgePoints = new(cameraPos.x - cameraHalfSize.x,
                                           cameraPos.x + cameraHalfSize.x,
                                           cameraPos.y - cameraHalfSize.y,
                                           cameraPos.y + cameraHalfSize.y);
            Vector4 tooltipEdgePoints = new(tooltipPos.x - tooltipHalfSize.x, 
                                            tooltipPos.x + tooltipHalfSize.x,
                                            tooltipPos.y - tooltipHalfSize.y,
                                            tooltipPos.y + tooltipHalfSize.y);

            if (_posOffset.x < 0 && tooltipEdgePoints.x < cameraEdgePoints.x) // left edge
                tooltipPos.x += (tooltipHalfSize.x - _posOffset.x) * 2;
            if (_posOffset.x > 0 && tooltipEdgePoints.y > cameraEdgePoints.y) // right edge
                tooltipPos.x -= (tooltipHalfSize.x + _posOffset.x) * 2;

            if (_posOffset.y < 0 && tooltipEdgePoints.z < cameraEdgePoints.z) // lower edge
                tooltipPos.y += (tooltipHalfSize.y - _posOffset.y) * 2;
            if (_posOffset.y > 0 && tooltipEdgePoints.w > cameraEdgePoints.w) // upper edge
                tooltipPos.y -= (tooltipHalfSize.y + _posOffset.y) * 2;

            _transform.position = tooltipPos;
        }

        void Start()
        {
            _gameObject = gameObject;
            _transform = _gameObject.transform;
            _renderer = _gameObject.GetComponent<SpriteRenderer>();
            _text = _gameObject.transform.GetChild(0).GetComponent<TextMeshPro>();
            _textRect = _text.GetComponent<RectTransform>();
        }
        void Update()
        {
            MoveToPointer();
        }
    }
}
