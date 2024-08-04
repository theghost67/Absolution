using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Environment;
using Game.Menus;
using Game.Palette;
using Game.Traits;
using GreenOne;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    /// <summary>
    /// Класс, содержащий неотъемлимые данные игрового процесса и инициализирующий основные игровые системы.<br/>
    /// Так же содержит данные конфигурации игры.
    /// </summary>
    public sealed class Global : MonoBehaviour
    {
        public const float NORMAL_SCALE = 100;
        public const float PIXEL_SCALE = 300;
        public const float NORMAL_TO_PIXEL = PIXEL_SCALE / NORMAL_SCALE;
        public const float ASPECT_RATIO = 16f / 9f;

        public static event Action OnUpdate;      // hz/s
        public static event Action OnFixedUpdate; // 50/s
        public static event Action OnSlowUpdate;  // 10/s
        public static event Action<bool> OnFocus;

        public static Transform Root => _root;
        public static Camera Camera => _camera;
        public static Volume Volume => _volume;
        public static bool IsQuitting => _isQuitting;

        public static bool writeConsoleLogs = true;
        public static bool shufflePrice = false;
        public static float soundVolumeScale = 1.0f; // set by player
        public static float musicVolumeScale = 1.0f; // in settings

        static readonly Color[] _palette = new Color[]
        {
            Utils.HexToColor("#eeeeee"),
            Utils.HexToColor("#76abae"),
            Utils.HexToColor("#39414f"),
            Utils.HexToColor("#222831"),
            Utils.HexToColor("#000000"),

            Utils.HexToColor("#00ffff"),
            Utils.HexToColor("#ffff00"),
        };
        static Transform _root;
        static Camera _camera;
        static Volume _volume;
        static bool _isQuitting;
        static int _slowCounter;

        static int _errorsCount;
        static Tween _errorTween;
        static GameObject _errorGameObject;
        static TextMeshPro _errorTextMesh;

        private Global() { }

        static void LogReceived(string condition, string stackTrace, LogType type)
        {
            if (_isQuitting) return;
            if (type != LogType.Exception && type != LogType.Error) return;

            _errorsCount++;
            _errorTween.Restart();
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                ShowErrors();
                TableConsole.LogToFile("global", condition);
            });
        }
        static bool OnWantToQuit()
        {
            _isQuitting = true;
            return true;
        }
        static bool OnTweenLog(LogType type, object log)
        {
            return type == LogType.Error || type == LogType.Exception;
        }

        static void ShowErrors()
        {
            _errorTextMesh.text = _errorsCount.ToString();
            _errorGameObject.SetActive(true);
        }
        static void HideErrors()
        {
            _errorGameObject.SetActive(false);
        }

        // TODO: remove
        Menu demo_CreateCardChoose(int stage)
        {
            CardChooseMenu menu = new(stage, 4, 0, 3, 6);
            menu.MenuWhenClosed = () => new BattlePlaceMenu();
            menu.OnClosed += menu.Destroy;
            return menu;
        }

        void Start()
        {
            _root = transform.root;
            _camera = Camera.main;
            _volume = _camera.GetComponent<Volume>();

            _errorTween = DOVirtual.DelayedCall(5, HideErrors);
            _errorGameObject = _root.Find("CORE/Errors").gameObject;
            _errorTextMesh = _errorGameObject.Find<TextMeshPro>("Text");

            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            Application.logMessageReceivedThreaded += LogReceived;
            Application.logMessageReceived += LogReceived;
            Application.wantsToQuit += OnWantToQuit;
            DOTween.onWillLog = OnTweenLog;
            //Time.fixedDeltaTime = 1 / 20f;

            TraitBrowser.Initialize();
            CardBrowser.Initialize();
            EnvironmentBrowser.Initialize();
            AudioBrowser.Initialize();
            TableConsole.Initialize();

            Player.Load();
            SaveSystem.LoadAll();
            for (int i = 0; i < _palette.Length; i++)
                ColorPalette.All[i].ColorAll = _palette[i];

            // --------- TODO: remove ---------
            Traveler.TryStartTravel(new LocationMission(EnvironmentBrowser.Locations["college"]));
            DOVirtual.DelayedCall(0.8f, () => VFX.CreateScreenBG(Color.black).DOFade(0, 10).SetEase(Ease.InQuad));
            MenuTransit.Between(null, demo_CreateCardChoose(EnvironmentBrowser.Locations["college"].stage));
            //#if UNITY_EDITOR
            //MenuTransit.Between(null, new BattlePlaceMenu());
            //         #else
            //MenuTransit.Between(null, demo_CreateCardChoose(EnvironmentBrowser.Locations["college"].stage));
            //#endif
            // --------------------------------
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            OnUpdate?.Invoke();
        }
        void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
            if (_slowCounter++ >= 10)
            {
                _slowCounter = 0;
                OnSlowUpdate?.Invoke();
            }
        }
        void OnApplicationFocus(bool focus)
        {
            OnFocus?.Invoke(focus);
        }
    }
}
