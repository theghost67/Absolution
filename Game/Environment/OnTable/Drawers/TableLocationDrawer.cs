using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableLocation"/>.
    /// </summary>
    public sealed class TableLocationDrawer : Drawer
    {
        static readonly GameObject _prefab;
        static readonly Sprite _lockedSprite;

        public readonly TableLocation attached;

        readonly SpriteRenderer _spriteRenderer;
        readonly Sprite _unlockedSprite;
        readonly Location _attachedData;
        bool _isUnlocked;

        static TableLocationDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Location icon");
            _lockedSprite = Resources.Load<Sprite>("Sprites/Locations/locked");
        }
        public TableLocationDrawer(TableLocation location, Transform parent) : base(location, _prefab, parent)
        {
            attached = location;

            _attachedData = attached.Data;
            _unlockedSprite = Resources.Load<Sprite>(_attachedData.iconPath);
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            UpdateByUnlockState();

            OnMouseEnter += OnLocationMouseEnter;
            OnMouseLeave += OnLocationMouseLeave;
            OnMouseClick += OnLocationMouseClick;
        }

        public void UpdateByUnlockState()
        {
            _isUnlocked = attached.IsUnlocked;

            if (_isUnlocked)
                 _spriteRenderer.sprite = _unlockedSprite;
            else _spriteRenderer.sprite = _lockedSprite;

            ChangePointer = _isUnlocked;
        }

        void OnLocationMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            //if (_isUnlocked) CreateSelection();
            //Tooltip.Show(_spriteRenderer, $"{_attachedData.name}\nУгроза: <color=red>{_attachedData.stage}</color> ед.");
        }
        void OnLocationMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            //if (_isUnlocked) DestroySelection();
            //Tooltip.Hide();
        }
        void OnLocationMouseClick(object sender, DrawerMouseEventArgs e)
        {
            //if (_isUnlocked)
            //    new MissionMenu(attached.Missions).TransitToThis();
            //else transform.DOAShake();
        }
    }
}
