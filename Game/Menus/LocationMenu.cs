using Cysharp.Threading.Tasks;
using Game.Effects;
using Game.Environment;
using GreenOne;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню для взаимодействия с текущей локацией (см. <see cref="Location"/>).
    /// </summary>
    public sealed class LocationMenu : Menu
    {
        static readonly GameObject _prefab;
        static readonly AlignSettings _alignSettings;
        readonly Location _location;

        readonly TextMeshPro _locationText;
        readonly TextMeshPro _descText;
        readonly TextMeshPro _travelText;

        readonly TextMeshPro _goldText;
        readonly TextMeshPro _healthText;

        readonly TextMeshPro _hintText;
        readonly Drawer _deckButton;

        readonly Transform _placesTransform;
        readonly HashSet<TableLocationPlace> _places;

        static LocationMenu()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Menus/Location");
            _alignSettings = new AlignSettings(Vector2.zero, AlignAnchor.MiddleCenter, TableLocationPlace.WIDTH * 2, true, 3);
        }
        public LocationMenu(Location location) : base("Location", _prefab)
        {
            _location = location;
            Traveler.OnTravelEnd += OnTravelEnd;

            _locationText = Transform.Find<TextMeshPro>("Header");
            _descText     = Transform.Find<TextMeshPro>("Left");
            _travelText   = Transform.Find<TextMeshPro>("Right");
            _goldText     = Transform.Find<TextMeshPro>("Gold");
            _healthText   = Transform.Find<TextMeshPro>("Health");
            _hintText     = Transform.Find<TextMeshPro>("Hint");

            TextMeshPro deckButtonText = Transform.Find<TextMeshPro>("Deck");
            _deckButton = new Drawer(null, deckButtonText).WithHoverTextEvents(deckButtonText);
            _deckButton.OnMouseClickLeft += (s, e) => new DeckMenu().OpenAnimated();

            _placesTransform = Transform.CreateEmptyObject("Places").transform;
            _places = new HashSet<TableLocationPlace>();
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            MusicPack.Get("Location").PlayFading();

            UpdateLocationText();
            UpdateDescText();
            UpdateTravelText();
            UpdateGoldText();
            UpdateHealthText();

            Traveler.TryContinueTravel();
            if (Traveler.IsTraveling)
                CreatePlaceIcons(Traveler.CurrentPlaces);
            else OnTravelFinished();
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            if (!Traveler.IsTraveling)
                MusicPack.Get("Location").StopFading();
        }
        public override void DestroyInstantly()
        {
            base.DestroyInstantly();
            Traveler.OnTravelEnd -= OnTravelEnd;
        }
        public override void SetColliders(bool value)
        {
            _deckButton.SetCollider(value);
            foreach (TableLocationPlace place in _places)
                place.Drawer.SetCollider(value);
        }

        async UniTaskVoid OnTravelFinished()
        {
            SetColliders(false);
            VFX.CreateText("ЛОКАЦИЯ ИССЛЕДОВАНА", Color.white, Transform).DOATextPopUp(delay: 2);

            await UniTask.Delay(3000);
            await WorldMenu.instance.OpenAnimated();
            OnTravelEnd();
        }
        void OnTravelEnd()
        {
            if (IsOpened) return;
            DestroyInstantly();
            MusicPack.Get("Location").StopInstantly();
        }

        void CreatePlaceIcons(LocationPlace[] places)
        {
            for (int i = 0; i < places.Length; i++)
            {
                if (places[i] == null) continue;
                TableLocationPlace place = new(places[i], _placesTransform);
                TableLocationPlaceDrawer placeDrawer = place.Drawer;
                placeDrawer.OnMouseEnter += (s, e) => ShowHint(place.Data.desc);
                placeDrawer.OnMouseLeave += (s, e) => HideHint();
                placeDrawer.OnMouseClickLeft += (s, e) => DestroyPlaceIcons();
                _places.Add(place);
            }

            _alignSettings.ApplyTo(_placesTransform);
        }
        void DestroyPlaceIcons()
        {
            foreach (TableLocationPlace place in _places)
                place.Drawer.TryDestroy(false);
            _places.Clear();
        }

        void ShowHint(string text)
        {
            _hintText.text = text;
        }
        void HideHint()
        {
            _hintText.text = string.Empty;
        }

        void UpdateLocationText()
        {
            _locationText.text = $"{_location.name}\n<size=50%>ур. угрозы: {Traveler.Mission.threatLevel.richName} ({Traveler.Location.stage} ед.)";
        }
        void UpdateDescText()
        {
            _descText.text = "Задания отсутствуют.";
        }
        void UpdateTravelText()
        {
            _travelText.text = $"Длительность: {Traveler.Mission.durationLevel.richName}\nПрогресс локации: {Traveler.CurrentProgress}/{Traveler.RequiredProgress}";
        }
        void UpdateGoldText()
        {
            _goldText.text = Player.Gold.ToString();
        }
        void UpdateHealthText()
        {
            _healthText.text = Player.HealthCurrent.ToString();
        }
    }
}
