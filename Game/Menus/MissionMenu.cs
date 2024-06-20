using Cysharp.Threading.Tasks;
using Game;
using Game.Environment;
using GreenOne;
using MyBox;
using System;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    // TODO[IMPORTANT]: update UI
    /// <summary>
    /// Класс, представляющий меню-оверлей для взаимодействия со случайно сгенерированными миссиями локаций. (см. <see cref="LocationMission"/>).
    /// </summary>
    [Obsolete("Should be rewritten completely")] public sealed class MissionMenu : Menu
    {
        const int MAXIMUM = 3;

        static readonly GameObject _prefab;
        static readonly AlignSettings _alignSettings;

        readonly Drawer[] _missionsDrawers;
        readonly Drawer _returnButton;
        readonly Drawer _deckButton;

        static MissionMenu()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Menus/Mission");
            _alignSettings = new AlignSettings(Vector2.zero, AlignAnchor.MiddleCenter, TableLocationMission.HEIGHT + 20, true, 1);
            _alignSettings.inversedAxes.y = true;
        }
        public MissionMenu(LocationMission[] data) : base("Mission", _prefab)
        {
            if (data.Length > MAXIMUM)
                throw new NotSupportedException($"Missions amount cannot be more than {MAXIMUM}.");

            Transform parent = Transform.CreateEmptyObject("Missions");
            _missionsDrawers = new Drawer[data.Length].FillBy(i => new TableLocationMission(data[i], parent).Drawer);
            _alignSettings.ApplyTo(parent);

            TextMeshPro returnButtonText = Transform.Find<TextMeshPro>("Return");
            _returnButton = new Drawer(null, returnButtonText).WithHoverTextEvents();
            _returnButton.OnMouseClickLeft += (s, e) => CloseAnimated();

            TextMeshPro deckButtonText = Transform.Find<TextMeshPro>("Deck");
            _deckButton = new Drawer(null, deckButtonText).WithHoverTextEvents();
            _deckButton.OnMouseClickLeft += (s, e) => new DeckMenu().OpenAnimated();
        }

        public override UniTask OpenAnimated()
        {
            throw new NotImplementedException();
        }
        public override UniTask CloseAnimated()
        {
            throw new NotImplementedException();
        }

        public override void SetColliders(bool value)
        {
            _returnButton.SetCollider(value);
            _deckButton.SetCollider(value);

            foreach (var drawer in _missionsDrawers)
                drawer.SetCollider(value);
        }
    }
}
