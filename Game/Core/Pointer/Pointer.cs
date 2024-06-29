using GreenOne;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///  ласс, представл€ющий указатель пользовател€.
    /// </summary>
    public class Pointer : MonoBehaviour
    {
        public static bool IsVisible { get => _isVisible; set => SetVisibility(value); }
        public static PointerType Type { get => _type; set => SetType(value); }
        public static Vector3 Position => _position;
        public static Transform Transform => _transform;

        static bool _isVisible;
        static PointerType _type;
        static Vector3 _position;
        static Sprite _typeSprite;

        static bool _update;
        static Pointer _instance;
        static SpriteRenderer _renderer;
        static Transform _transform;

        [SerializeField] Sprite _spriteNormal;
        [SerializeField] Sprite _spriteHand;
        [SerializeField] Sprite _spriteAiming;

        private Pointer() { }
        public static void Redraw()
        {
            _instance.FixedUpdate();
        }

        static void SetVisibility(bool value)
        {
            _isVisible = value;
            _renderer.enabled = value;
        }
        static void SetType(PointerType type)
        {
            _type = type;
            switch (type)
            {
                case PointerType.Normal: _instance.SetAsNormal(); break;
                case PointerType.Hand: _instance.SetAsHand(); break;
                case PointerType.Aiming: _instance.SetAsAiming(); break;
                default: throw new System.NotSupportedException();
            }
        }

        void SetAsNormal()
        {
            _type = PointerType.Normal;
            _typeSprite = _spriteNormal;
            _renderer.sprite = _typeSprite;
        }
        void SetAsHand()
        {
            _type = PointerType.Hand;
            _typeSprite = _spriteHand;
            _renderer.sprite = _typeSprite;
        }
        void SetAsAiming()
        {
            _type = PointerType.Aiming;
            _typeSprite = _spriteAiming;
            _renderer.sprite = _typeSprite;
        }

        void Awake()
        {
            _instance = this;
            _transform = transform;
            _renderer = _transform.Find("Sprite").GetComponent<SpriteRenderer>();

            Cursor.visible = false;

            SetVisibility(true);
            SetType(PointerType.Normal);
        }
        void OnApplicationFocus(bool focus)
        {
            _update = focus;
        }

        void Update()
        {
            if (!_update) return;
            _position = Utils.MouseToWorldPos(Global.Camera, 0);
            transform.position = _position;
        }
        void FixedUpdate()
        {
            if (!_update) return;
            bool changePointer = false;
            foreach (Drawer drawer in Drawer.SelectedDrawers)
            {
                if (!drawer.ChangePointer) continue;
                changePointer = true;
                break;
            }

            if (_type == default && changePointer)
                 _renderer.sprite = _spriteHand;
            else _renderer.sprite = _typeSprite;
        }
    }
}