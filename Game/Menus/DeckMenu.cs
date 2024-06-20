using Game.Cards;
using GreenOne;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню для взаимодействия с колодой игрока (см. <see cref="CardDeck"/>).
    /// </summary>
    public sealed class DeckMenu : Menu
    {
        static readonly GameObject _prefab;
        static readonly AlignSettings _alignSettings;

        readonly Transform _cardsTransform;
        readonly TextMeshPro _limitText;
        readonly TextMeshPro _totalText;
        readonly Drawer _returnButton;

        TableCard[] _cards;

        static DeckMenu()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Menus/Deck");
            _alignSettings = new AlignSettings(Vector2.zero, AlignAnchor.MiddleCenter, new float2(8 + TableCardDrawer.WIDTH, 8 + TableCardDrawer.HEIGHT), true, 12);
        }
        public DeckMenu() : base("Deck", _prefab)
        {
            _cardsTransform = Transform.CreateEmptyObject("Cards").transform;
            _limitText = Transform.Find<TextMeshPro>("Left");
            _totalText = Transform.Find<TextMeshPro>("Right");

            TextMeshPro returnButtonText = Transform.Find<TextMeshPro>("Return");
            _returnButton = new Drawer(null, returnButtonText.gameObject).WithHoverTextEvents(returnButtonText);
            _returnButton.OnMouseClickLeft += (s, e) => DestroyAnimated();
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            CreateCards();

            _limitText.text = $"Количество карт: {Player.Deck.Count}/{CardDeck.LIMIT}\nКоличество карт полей: {Player.Deck.fieldCards.Count}";
            _totalText.text = $"Количество характеристик: {Player.Deck.Points}\nКоличество трейтов: {Player.Deck.Traits}";
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            DestroyCards();
        }
        public override void SetColliders(bool value)
        {
            _returnButton.SetCollider(value);
            foreach (TableCard card in _cards)
                card.Drawer.SetCollider(value);
        }

        void CreateCards()
        {
            int index = 0;
            _cards = new TableCard[Player.Deck.Count];

            foreach (Card card in Player.Deck)
            {
                TableCard tCard = card.CreateOnTable(parent: _cardsTransform);
                TableCardDrawer tCardDrawer = tCard.Drawer;
                tCardDrawer.SetSortingOrder(2 * index, asDefault: true);
                tCardDrawer.ChangePointer = true;
                tCardDrawer.OnMouseEnter += (s, e) => tCardDrawer.SetSortingAsTop();
                tCardDrawer.OnMouseLeave += (s, e) => tCardDrawer.SetSortingAsDefault();
                _cards[index] = tCard;
                index++;
            }

            if (Player.Deck.Count >= 12)
                _alignSettings.distance.x = TableCardDrawer.WIDTH - 29;
            else if (Player.Deck.Count > 4)
                 _alignSettings.distance.x = TableCardDrawer.WIDTH + 8 - (TableCardDrawer.WIDTH / 12 * (Player.Deck.Count - 4));
            else _alignSettings.distance.x = TableCardDrawer.WIDTH + 8;

            _alignSettings.ApplyTo(_cardsTransform);
        }
        void DestroyCards()
        {
            _cards = null;
            foreach (Transform child in _cardsTransform)
                child.gameObject.Destroy();
        }
    }
}
