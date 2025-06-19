using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, представляющий методы для отображения подсказок.
    /// </summary>
    public sealed class Tooltip : MonoBehaviour
    {
        const KeyCode LINKS_KEY = KeyCode.LeftAlt;

        static readonly Vector2 _tooltipSizeLimit = new(1.2f, 1.6f);
        static readonly Vector2 _tooltipsMarginSum = new(0.04f * 2, 0.02f * 2);
        static readonly Vector3 _tooltipsOffset = new(0.02f, 0.02f);

        static Transform _parent;
        static List<Prefab> _prefabs = new();
        static HorizontalAlignmentOptions _align;
        static string[] _texts = Array.Empty<string>();

        class Prefab
        {
            static readonly GameObject _base = Resources.Load<GameObject>("Prefabs/Tooltip");
            readonly GameObject _gameObject;
            readonly Transform _transform;
            readonly SpriteRenderer _renderer;
            readonly TextMeshPro _text;
            readonly RectTransform _textRect;
            bool _isVisible;

            public Prefab(Transform parent)
            {
                _gameObject = GameObject.Instantiate(_base, parent);
                _transform = _gameObject.transform;
                _renderer = _gameObject.GetComponent<SpriteRenderer>();
                _text = _gameObject.transform.GetChild(0).GetComponent<TextMeshPro>();
                _textRect = _text.GetComponent<RectTransform>();
                SetAlign(_align);
            }
            public void Destroy()
            {
                GameObject.Destroy(_gameObject);
            }

            public void SetText(string text)
            {
                _text.text = text;
            }
            public void Show()
            {
                if (string.IsNullOrEmpty(_text.text)) return;

                Vector2 optimalSize = GetForceUpdatedTextSize(_text.text, _tooltipSizeLimit);
                Vector2 optimalSizeWithMargin = _tooltipsMarginSum + optimalSize;

                _isVisible = true;
                _gameObject.SetActive(true);
                _renderer.size = optimalSizeWithMargin;
                _textRect.sizeDelta = optimalSize;
                _text.horizontalAlignment = _align;
            }
            public void Hide()
            {
                _isVisible = false;
                _gameObject.SetActive(false);
            }
            public void MoveToCornerLocal(Vector3 pos)
            {
                _transform.localPosition = new Vector3(pos.x + _renderer.size.x / 2, pos.y - _renderer.size.y / 2);
            }

            public Vector3 GetSize()
            {
                return _renderer.size;
            }
            public bool IsVisible()
            {
                return _isVisible;
            }

            Vector2 GetForceUpdatedTextSize(string text, Vector2 sizeDelta)
            {
                _textRect.sizeDelta = sizeDelta;
                _text.text = text;
                _text.ForceMeshUpdate(ignoreActiveState: true);
                return _text.textBounds.size;
            }
        }

        public static void SetAlign(HorizontalAlignmentOptions align)
        {
            _align = align;
        }
        public static void SetText(params string[] texts)
        {
            if (texts.Length == 0)
            {
                ClearText();
                return;
            }

            _texts = texts;
            if (texts.Length == 1)
            {
                foreach (Prefab prefab in _prefabs)
                    prefab.Hide();
                _prefabs[0].SetText(texts[0]);
                _prefabs[0].MoveToCornerLocal(Vector3.zero);
                MoveToPointerGlobal();
                return;
            }

            for (int i = 0; i < texts.Length; i++)
            {
                if (_prefabs.TryGetValue(i) == null)
                    _prefabs.Add(new Prefab(_parent));
                _prefabs.TryGetValue(i).SetText(texts[i]);
            }
            foreach (Prefab prefab in _prefabs)
                prefab.Hide();

            MoveToPointerGlobal();
        }
        public static void SetTextImmediately(string text)
        {
            foreach (Prefab prefab in _prefabs)
                prefab.Hide();
            _prefabs[0].SetText(text);
            _prefabs[0].MoveToCornerLocal(Vector3.zero);
            _prefabs[0].Show();
            MoveToPointerGlobal();
        }
        public static void ClearText()
        {
            _texts = Array.Empty<string>();
            foreach (Prefab prefab in _prefabs)
                prefab.Hide();
        }

        static Vector3 OrganizeTooltips()
        {
            Vector3 maxSize = Vector3.zero;
            Vector3 curPos = Vector3.zero;
            foreach (Prefab prefab in _prefabs)
            {
                if (!prefab.IsVisible()) continue;
                Vector3 prefabSize = prefab.GetSize();
                prefab.MoveToCornerLocal(curPos);
                curPos.y -= prefabSize.y + _tooltipsOffset.y;
                if (-curPos.y > _tooltipSizeLimit.y)
                {
                    curPos.x = maxSize.x + _tooltipsOffset.x;
                    curPos.y = 0;
                    prefab.MoveToCornerLocal(curPos);
                }
                maxSize.y = Mathf.Max(maxSize.y, -curPos.y);
                maxSize.x = Mathf.Max(maxSize.x, prefabSize.x);
            }
            return maxSize;
        }
        static void MoveToPointerGlobal()
        {
            if (!_prefabs[0].IsVisible()) return;

            Vector3 cameraHalfSize = new(Global.Camera.orthographicSize * Global.ASPECT_RATIO, Global.Camera.orthographicSize);
            Vector3 tooltipSizeScaled = OrganizeTooltips() * Global.PIXEL_SCALE;
            Vector3 offset = new Vector3(0.04f, -0.04f) * Global.PIXEL_SCALE;

            Vector3 pointerPos = Pointer.Position;
            Vector3 cameraPos = Global.Camera.transform.position;
            Vector3 tooltipPos = pointerPos + offset;

            float cameraRightEdge = cameraPos.x + cameraHalfSize.x;
            float cameraLowerEdge = cameraPos.y - cameraHalfSize.y;

            float tooltipRightEdge = tooltipPos.x + tooltipSizeScaled.x;
            float tooltipLowerEdge = tooltipPos.y - tooltipSizeScaled.y;

            if (tooltipRightEdge > cameraRightEdge)
                tooltipPos.x = tooltipPos.x - offset.x * 2 - tooltipSizeScaled.x;
            if (tooltipLowerEdge < cameraLowerEdge)
                tooltipPos.y = tooltipPos.y - offset.y + tooltipSizeScaled.y;

            _parent.position = tooltipPos;
        }
        static void OnUpdate()
        {
            if (_texts.Length == 0) return;
            if (Input.GetKeyDown(LINKS_KEY))
            {
                if (_prefabs.Count > 0 && _prefabs[0].IsVisible())
                     Hide();
                else Show();
            }
        }

        static void Show()
        {
            for (int i = 0; i < _texts.Length; i++)
                _prefabs[i].Show();
        }
        static void Hide()
        {
            for (int i = 0; i < _texts.Length; i++)
                _prefabs[i].Hide();
        }

        void Start()
        {
            _parent = transform;
            _prefabs.Add(new Prefab(_parent));
            _prefabs[0].Hide();
            Global.OnUpdate += OnUpdate;
        }
        void Update()
        {
            MoveToPointerGlobal();
        }
    }
}
