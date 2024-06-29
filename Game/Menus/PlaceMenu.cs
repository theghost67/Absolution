using Game.Effects;
using Game.Environment;
using GreenOne;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Абстрактный класс, представляющий меню для взаимодействия с местом локации (см. <see cref="LocationPlace"/>).
    /// </summary>
    public abstract class PlaceMenu : Menu, ITableEntrySource
    {
        public string TableName => Id;
        public string TableNameDebug => Id;

        protected abstract string PlaceId { get; }
        protected abstract string HeaderText { get; }
        protected abstract string LeftText { get; }
        protected abstract string RightText { get; }

        static readonly GameObject _prefab;

        readonly TextMeshPro _headerText;
        readonly TextMeshPro _leftText;
        readonly TextMeshPro _rightText;

        readonly TextMeshPro _goldText;
        readonly TextMeshPro _healthText;

        readonly Drawer _deckButton;
        readonly Drawer _leaveButton;

        int IUnique.Guid => 0x0ACE;
        string IUnique.GuidStr => 0x0ACE.ToString();

        /// <summary>
        /// Содержит флаги, позволяющие определить элементы графического интерфейса у меню.
        /// </summary>
        protected enum UIFlags
        {
            WithNothing,

            WithHeaderTexts = 1,
            WithUpperTexts = 2,
            WithLowerTexts = 4,
            WithAllTexts = WithHeaderTexts | WithUpperTexts | WithLowerTexts,

            WithLeaveButton = 8,
            WithDeckButton = 16,
            WithAllButtons = WithLeaveButton | WithDeckButton,

            WithAll = WithAllTexts | WithAllButtons,
        }

        static PlaceMenu()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Menus/Place");
        }

        protected PlaceMenu(string id, UIFlags flags) : this(id, flags, _prefab) { }
        protected PlaceMenu(string id, UIFlags flags, GameObject prefab) : base(id, prefab)
        {
            // TODO: remove?
            //Transform.Find<SpriteRenderer>("BG").sprite = Resources.Load<Sprite>($"Sprites/Backgrounds/{PlaceId}");

            if (flags.HasFlag(UIFlags.WithHeaderTexts))
            {
                _headerText = Transform.Find<TextMeshPro>("Header");
                _headerText.text = HeaderText;
            }

            if (flags.HasFlag(UIFlags.WithUpperTexts))
            {
                _leftText = Transform.Find<TextMeshPro>("Left");
                _rightText = Transform.Find<TextMeshPro>("Right");

                string left = LeftText;
                string right = RightText;

                _leftText.DOATextTyping(left, left.Length * 0.05f);
                _rightText.DOATextTyping(right, right.Length * 0.05f);
            }

            if (flags.HasFlag(UIFlags.WithLowerTexts))
            {
                _goldText = Transform.Find<TextMeshPro>("Gold");
                _healthText = Transform.Find<TextMeshPro>("Health");

                Player.OnGoldSet += UpdateGoldText;
                Player.OnHealthSet += UpdateHealthText;

                UpdateGoldText(Player.Gold);
                UpdateHealthText(Player.HealthCurrent);
            }

            if (flags.HasFlag(UIFlags.WithDeckButton))
            {
                _deckButton = new Drawer(null, Transform.Find<TextMeshPro>("Deck")).WithHoverTextEvents();
                _deckButton.OnMouseClickLeft += (s, e) => new DeckMenu().OpenAnimated();
            }

            if (flags.HasFlag(UIFlags.WithLeaveButton))
            {
                _leaveButton = new Drawer(null, Transform.Find<TextMeshPro>("Leave")).WithHoverTextEvents();
                _leaveButton.OnMouseClickLeft += (s, e) => CloseAnimated();
            }
        }

        public override void SetColliders(bool value)
        {
            base.SetColliders(value);
            _deckButton?.SetCollider(value);
            _leaveButton?.SetCollider(value);
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            Player.OnGoldSet -= UpdateGoldText;
            Player.OnHealthSet -= UpdateHealthText;
        }

        void UpdateGoldText(int value)
        {
            _goldText.text = value.ToString();
        }
        void UpdateHealthText(int value)
        {
            _healthText.text = value.ToString();
        }
    }
}
