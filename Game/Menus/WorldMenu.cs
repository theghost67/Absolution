using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game;
using Game.Environment;
using GreenOne;
using MyBox;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий меню для взаимодействия с миром (см. <see cref="World"/>).
    /// </summary>
    public sealed class WorldMenu : Menu
    {
        public static readonly WorldMenu instance;

        static readonly GameObject _prefab;
        static readonly Color _arrowInactiveColor;

        readonly TableLocation[] _locations;
        readonly Vector3[] _locationPositions;

        readonly TextMeshPro _daysText;
        readonly SpriteRenderer[] _arrows;
        readonly Tweener[] _arrowsTweeners;

        static WorldMenu()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Menus/World");
            _arrowInactiveColor = new Color(0.4f, 0.4f, 0.4f);
            instance = new WorldMenu();
        }
        private WorldMenu() : base("Мир", _prefab)
        {
            _locations = new TableLocation[Location.COUNT];
            _locationPositions = new Vector3[Location.COUNT]
            {
                new Vector3(-2.21f, -1.42f, 0) * Global.PIXEL_SCALE,
                new Vector3(-0.7f, -1f, 0) * Global.PIXEL_SCALE,
                new Vector3(0.88f, -1.24f, 0) * Global.PIXEL_SCALE,
                new Vector3(1.94f, -0.2f, 0) * Global.PIXEL_SCALE,
                new Vector3(0.46f, 0.15f, 0) * Global.PIXEL_SCALE,

                new Vector3(-1.13f, -0.23f, 0) * Global.PIXEL_SCALE,
                new Vector3(-2.15f, 0.71f, 0) * Global.PIXEL_SCALE,
                new Vector3(-0.58f, 1.24f, 0) * Global.PIXEL_SCALE,
                new Vector3(1.03f, 1.04f, 0) * Global.PIXEL_SCALE,
                new Vector3(2.52f, 1.27f, 0) * Global.PIXEL_SCALE,
            };

            _daysText = Transform.Find<TextMeshPro>("Days");

            _arrows = new SpriteRenderer[Location.COUNT - 1].FillBy(i => Transform.Find<SpriteRenderer>($"Arrow {i}"));
            _arrowsTweeners = new Tweener[Location.COUNT - 1];

            CreateLocationIcons();
            DOVirtual.DelayedCall(4, () => TweenRouteArrows());
        }

        public override UniTask OpenAnimated()
        {
            throw new System.NotImplementedException();
        }
        public override UniTask CloseAnimated()
        {
            throw new System.NotImplementedException();
        }

        public override void OpenInstantly()
        {
            base.OpenInstantly();
            TweenRouteArrows();

            MusicPack.Get("World").PlayFading().Forget();
            _daysText.text = $"ДЕНЬ {World.Days}";

            // TODO: replace with "foreach" loop when finished all
            for (int i = 0; i < Location.COUNT_FINISHED; i++)
                _locations[i].Drawer.UpdateByUnlockState();
        }
        public override void CloseInstantly()
        {
            base.CloseInstantly();
            KillRouteArrows();
        }
        public override void SetColliders(bool value)
        {
            foreach (TableLocation location in _locations)
                location.Drawer.SetCollider(value);
        }

        async UniTaskVoid TweenRouteArrows()
        {
            for (int i = 0; i < _arrows.Length; i++)
            {
                var arrow = _arrows[i];
                    arrow.color = Color.white;

                if (!_locations[i + 1].IsUnlocked)
                {
                    _arrowsTweeners[i] = arrow.DOColor(_arrowInactiveColor, 0.75f);
                    await UniTask.Delay(500);
                    continue;
                }

                Tweener tweener = DOVirtual.Float(0, 1, 128, v =>
                {
                    if (Random.Range(0, 16) == 0)
                        arrow.color = _arrowInactiveColor;
                    else arrow.color = Color.white;
                });
                tweener.SetUpdate(UpdateType.Fixed);
                tweener.OnComplete(() => tweener.Restart());

                _arrowsTweeners[i] = tweener;
                await UniTask.Delay(500);
            }
        }
        void KillRouteArrows()
        {
            for (int i = 0; i < _arrows.Length; i++)
            {
                _arrows[i].color = _arrowInactiveColor;
                _arrowsTweeners[i].Kill();
            }
        }
        void CreateLocationIcons()
        {
            int index = 0;
            Transform parent = Transform.CreateEmptyObject("Locations");

            foreach (Location location in EnvironmentBrowser.Locations.Values)
            {
                TableLocation tLocation = new(location, parent);
                tLocation.Drawer.transform.position = _locationPositions[index];
                _locations[index] = tLocation;
                index++;
            }
        }
    }
}
