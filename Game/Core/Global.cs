using DG.Tweening;
using Game.Cards;
using Game.Environment;
using Game.Menus;
using Game.Traits;
using GreenOne;
using System;
using TMPro;
using UnityEngine;

namespace Game
{
    // DEBUG NOTE 1: use TableFinder.storeNames to check the names of failed to find objects
    // DEBUG NOTE 1: TableTerritory.WriteLog() will provide more info if TableTerritory.fullLogs == true
    // by default, set to true in cloned territories

    /// <summary>
    /// Класс, содержащий неотъемлимые данные игрового процесса и инициализирующий основные игровые системы.
    /// </summary>
    public sealed class Global : MonoBehaviour
    {
        public const float NORMAL_SCALE = 100;
        public const float PIXEL_SCALE = 300;
        public const float NORMAL_TO_PIXEL = PIXEL_SCALE / NORMAL_SCALE;
        public const float ASPECT_RATIO = 16f / 9f;

        public static event Action OnUpdate;      // HZ/s
        public static event Action OnFixedUpdate; // 50/s
        public static event Action OnSlowUpdate;  // 10/s

        public static Transform Root => _root;
        public static Camera Camera => _camera;

        static Transform _root;
        static Camera _camera;
        static bool _quitting;
        static int _slowCounter;

        static int _errorsCount;
        static Tween _errorTween;
        static GameObject _errorGameObject;
        static TextMeshPro _errorTextMesh;

        private Global() { }

        static void LogReceived(string condition, string stackTrace, LogType type)
        {
            if (_quitting) return;
            if (type == LogType.Exception || type == LogType.Error)
            {
                _errorsCount++;
                _errorTween.Restart();
                ShowErrors();
            }
        }
        static bool OnWantToQuit()
        {
            _quitting = true;
            return true;
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
        static void OpenCardChoose()
        {
            CardChooseMenu menu = new();
            menu.cardStatPoints = EnvironmentBrowser.Locations["college"].stage;
            menu.choicesLeft = 2;
            menu.cardsCount = 2;
            menu.OpenAnimated();
            menu.OnClosed += OpenCardUpgrade;
        }
        static void OpenCardUpgrade()
        {
            CardUpgradeMenu menu = new(Player.Deck, 150, false);
            menu.OpenAnimated();
            menu.OnClosed += () => Traveler.TryStartDEMO(EnvironmentBrowser.Locations["college"]);
        }

        void Start()
        {
            _camera = Camera.main;
            _root = _camera.transform.root;

            _errorTween = DOVirtual.DelayedCall(5, HideErrors);
            _errorGameObject = _root.Find("CORE/Errors").gameObject;
            _errorTextMesh = _errorGameObject.Find<TextMeshPro>("Text");

            Application.logMessageReceived += LogReceived;
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value; // TODO: add in settings
            Application.wantsToQuit += OnWantToQuit;
            //Time.fixedDeltaTime = 1 / 20f;

            TraitBrowser.Initialize();
            CardBrowser.Initialize();
            EnvironmentBrowser.Initialize();
            Player.Load();

            SaveSystem.LoadAll();
            //MusicPack.Initiailize();
            //WorldMenu.instance.OpenAnimated();

            TableConsole.Initialize();
            OpenCardChoose();
            //DOVirtual.DelayedCall(2, OpenCardChoose); // OpenCardChoose
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
    }
}
