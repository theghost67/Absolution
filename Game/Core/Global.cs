﻿using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Environment;
using Game.Menus;
using Game.Traits;
using GreenOne;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using TMPro;
using UnityEngine;

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

        public static Transform Root => _root;
        public static Camera Camera => _camera;

        public static bool writeConsoleLogs = true;
        public static bool shufflePrice = false;
        public static float soundVolumeScale = 1.0f; // set by player
        public static float musicVolumeScale = 1.0f; // in settings

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
            if (type != LogType.Exception && type != LogType.Error) return;

            _errorsCount++;
            _errorTween.Restart();
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ShowErrors();
                const string TERMINATOR = "////////////////////////";
                TableConsole.LogToFile($"{TERMINATOR} {condition} {TERMINATOR}");
                TableConsole.Log($"{condition}\nСм. лог отладки для подробностей.", type);
            });
        }
        static bool OnWantToQuit()
        {
            _quitting = true;
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
            CardChooseMenu menu = new(stage, 4, 0, 3, 4);
            menu.MenuWhenClosed = () => new BattlePlaceMenu();
            menu.OnClosed += menu.DestroyInstantly;
            return menu;
        }

        void Start()
        {
            _camera = Camera.main;
            _root = _camera.transform.root;

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

            Traveler.TryStartTravel(new LocationMission(EnvironmentBrowser.Locations["college"]));

            //MenuTransit.Between(null, demo_CreateCardChoose(EnvironmentBrowser.Locations["college"].stage));
            MenuTransit.Between(null, new BattlePlaceMenu());
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
