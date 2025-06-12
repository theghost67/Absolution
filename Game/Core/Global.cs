using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using Game.Palette;
using Game.Traits;
using GreenOne;
using System;
using System.IO;
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

        public static Color[] Palette => _palette;
        public static MainMenu Menu => _menu;
        public static Transform Root => _root;
        public static Camera Camera => _camera;
        public static Volume Volume => _volume;
        public static bool IsQuitting => _isQuitting;

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

        static MainMenu _menu;
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
            TableConsole.LogToFile("global", condition);
            UnityMainThreadDispatcher.Enqueue(ShowErrors);
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
        static void UpdateCameraSize()
        {
            float aspectCurrent = (float)Screen.width / Screen.height;
            float size = 540;
            if (aspectCurrent < ASPECT_RATIO)
                 size *= ASPECT_RATIO / aspectCurrent;
            _camera.orthographicSize = size;
        }

        static void DisableCards()
        {
            if (!File.Exists("disabled_cards.txt")) return;
            string[] ids = File.ReadAllLines("disabled_cards.txt");
            foreach (string id in ids)
            {
                CardBrowser.AllIndexed.TryGetValue(id, out Card card);
                if (card == null) continue;
                card.frequency = 0;
                TableConsole.Log($"Карта отключена: {id}.", LogType.Log);
            }
        }
        static void DisableTraits()
        {
            if (!File.Exists("disabled_traits.txt")) return;
            string[] ids = File.ReadAllLines("disabled_traits.txt");
            foreach (string id in ids)
            {
                TraitBrowser.AllIndexed.TryGetValue(id, out Trait trait);
                if (trait == null) continue;
                trait.frequency = 0;
                TableConsole.Log($"Навык отключён: {id}.", LogType.Log);
            }
        }

        void Awake()
        {
            _root = transform.root;
            _camera = Camera.main;
            _volume = _camera.GetComponent<Volume>();
        }
        void Start()
        {
            _errorTween = DOVirtual.DelayedCall(5, HideErrors);
            _errorGameObject = _root.Find("CORE/Errors").gameObject;
            _errorTextMesh = _errorGameObject.Find<TextMeshPro>("Text");

            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            Application.logMessageReceivedThreaded += LogReceived;
            Application.logMessageReceived += LogReceived;
            Application.wantsToQuit += OnWantToQuit;
            DOTween.onWillLog = OnTweenLog;
            //Time.fixedDeltaTime = 1 / 20f;

            UpdateCameraSize();
            //Translator.Initialize(); // TODO: restore
            TraitBrowser.Initialize();
            CardBrowser.Initialize();
            AudioBrowser.Initialize();
            TableConsole.Initialize();

            DisableCards();
            DisableTraits();

            for (int i = 0; i < ColorPalette.All.Length; i++)
                ColorPalette.Current = _palette;

            VFX.CreateScreenBG(Color.black).DOFade(0, 10).SetEase(Ease.InQuad);
            SFX.PlayMusicMix("main");
            _menu = new MainMenu();
            _menu.TryOpen();
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
            if (focus) UpdateCameraSize();
            OnFocus?.Invoke(focus);
        }
    }
}
