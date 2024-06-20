using Cysharp.Threading.Tasks;
using Game;
using GreenOne;
using System;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню-оверлей для отображения всплывающего окна пользователю.
    /// </summary>
    public sealed class MessageMenu : Menu
    {
        static readonly GameObject _prefab;
        static readonly GameObject _prefabForButtons;

        readonly TextMeshPro _headerText;
        readonly TextMeshPro _descText;

        readonly Drawer _upperButton;
        readonly Drawer _lowerButton;

        /// <summary>
        /// Класс, представляющий одну из кнопок меню.
        /// </summary>
        public class Button
        {
            public readonly string text;
            public readonly Action OnClicked;
            public Button(string text, Action onClicked)
            {
                this.text = text;
                this.OnClicked = onClicked;
            }
        }

        static MessageMenu()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Menus/Message");
            _prefabForButtons = Resources.Load<GameObject>("Prefabs/Message button");
        }
        public MessageMenu(string header, string desc) : base("Message", _prefab)
        {
            int sortingOrder = 600 + OpenDepth;

            _headerText = Transform.Find<TextMeshPro>("Header");
            _headerText.text = header;
            _headerText.sortingOrder = sortingOrder;

            _descText = Transform.Find<TextMeshPro>("Desc");
            _descText.text = desc;
            _descText.sortingOrder = sortingOrder;
        }

        public override UniTask OpenAnimated()
        {
            OpenInstantly();
            return UniTask.CompletedTask;
        }
        public override UniTask CloseAnimated()
        {
            CloseInstantly();
            return UniTask.CompletedTask;
        }

        public override void SetColliders(bool value)
        {
            _upperButton.SetCollider(value);
            _lowerButton.SetCollider(value);
        }
        public void CreateButtons(params Button[] buttons)
        {
            // TODO: update values
            const float Y_START_POS = -320;
            const float Y_DIST_BETWEEN_BUTTONS = 16;

            int sortingOrder = 600 + OpenDepth;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                Drawer buttonDrawer = new(_prefabForButtons, Transform);
                buttonDrawer.transform.position = Vector2.up * (Y_START_POS + Y_DIST_BETWEEN_BUTTONS * i);
                buttonDrawer.gameObject.GetComponent<TextMeshPro>().text = button.text;
                buttonDrawer.SetSortingOrder(sortingOrder);
                buttonDrawer.OnMouseClickLeft += (s, e) => button.OnClicked();
                buttonDrawer.WithHoverTextEvents();
            }
        }
    }
}
