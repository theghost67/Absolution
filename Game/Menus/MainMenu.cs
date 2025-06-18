using DG.Tweening;
using Game.Effects;
using Game.Palette;
using GreenOne;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    public class MainMenu : Menu
    {
        const string ID = "main";
        public override string LinkedMusicMixId => "main";

        static readonly GameObject _prefab;
        static readonly string[] _tips;

        readonly TextMeshPro _psychoModeTMP;
        readonly TextMeshPro _versionTMP;
        readonly TextMeshPro _leaderboardsTMP;
        readonly TextMeshPro _controlsTMP;
        readonly TextMeshPro _tipsTMP;

        readonly GameObject _psychoModeGO;
        readonly GameObject _versionGO;
        readonly GameObject _leaderboardsGO;
        readonly GameObject _controlsGO;
        readonly GameObject _tipsGO;

        readonly LogoDrawer _logoDrawer;
        readonly LanguageDrawer _langDrawer;

        static Tween _flickeringTween;
        static Tween _tipsTween;
        static List<int> _tipsIndecies;
        static bool _firstTimeOpened;
        static Color _logoInitialColor;
        static float _logoLastIntensity;

        class LogoDrawer : Drawer
        {
            readonly MainMenu _menu;
            SpriteRenderer _renderer;
            Tween _scaleTween;

            public LogoDrawer(MainMenu menu) : base(null, menu.Transform.Find("Logo"))
            { 
                _menu = menu;
                _renderer = gameObject.GetComponent<SpriteRenderer>();
                ChangePointer = true;
                Color = Color.white;
            }

            protected override void SetColor(Color value)
            {
                base.SetColor(value);
                _renderer.material.SetColor("_Color", value);
            }

            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseEnterBase(sender, e);
                _scaleTween.Kill();
                _scaleTween = transform.DOScale(0.85f, 0.33f).SetEase(Ease.OutQuad);
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseLeaveBase(sender, e);
                _scaleTween.Kill();
                _scaleTween = transform.DOScale(0.75f, 0.33f).SetEase(Ease.OutQuad);
            }
            protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseClickBase(sender, e);
                _menu.StartTheGame();
            }
        }
        class LanguageDrawer : Drawer
        {
            readonly string[] _languages;
            int _langIndex;
            SpriteRenderer _renderer;
            TextMeshPro _text;

            public LanguageDrawer(MainMenu menu) : base(null, menu.Transform.Find("Language icon"))
            {
                _languages = Translator.SupportedLanguages;
                _langIndex = Translator.SupportedLanguages.FirstIndex(s => s == Translator.CurrentLanguage);
                _renderer = gameObject.GetComponent<SpriteRenderer>();
                _text = menu.Transform.Find<TextMeshPro>("Language");
                ChangePointer = true;
                Color = Color.white;

                string language = Translator.SupportedLanguages[_langIndex];
                _renderer.sprite = Resources.Load<Sprite>($"Localization/Icons/{language}");
            }
            protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseClickBase(sender, e);
                _langIndex = (_langIndex + 1) % _languages.Length;
                string language = Translator.SupportedLanguages[_langIndex];
                _renderer.sprite = Resources.Load<Sprite>($"Localization/Icons/{language}");
                _text.text = "RESTART TO APPLY";
                Translator.CurrentLanguage = language;
            }
        }

        static MainMenu() 
        {
            _logoLastIntensity = 1;
            _logoInitialColor = Color.white;
            _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
            _tips = new string[]
            {
                Translator.GetString("main_menu_1"),
                Translator.GetString("main_menu_2"),
				Translator.GetString("main_menu_3"),
                Translator.GetString("main_menu_4"),
                Translator.GetString("main_menu_5"),
                Translator.GetString("main_menu_6"),
                Translator.GetString("main_menu_7"),
                Translator.GetString("main_menu_8"),
                Translator.GetString("main_menu_9"),
                Translator.GetString("main_menu_10"),
                Translator.GetString("main_menu_11"),
                Translator.GetString("main_menu_12"),
                Translator.GetString("main_menu_13"),
                Translator.GetString("main_menu_14"),
                Translator.GetString("main_menu_15"),
                Translator.GetString("main_menu_16"),
				Translator.GetString("main_menu_17"),
                Translator.GetString("main_menu_18"),
                Translator.GetString("main_menu_19"),
                Translator.GetString("main_menu_20"),
                Translator.GetString("main_menu_21"),
                Translator.GetString("main_menu_22"),
                Translator.GetString("main_menu_23"),
                Translator.GetString("main_menu_24"),
                Translator.GetString("main_menu_25"),
                Translator.GetString("main_menu_26")
            };
            _tipsIndecies = Enumerable.Range(0, _tips.Length).ToList();
            _firstTimeOpened = true;
        }
        public MainMenu() : base(ID, _prefab)
        {
            _logoDrawer = new LogoDrawer(this);
            _langDrawer = new LanguageDrawer(this);

            _psychoModeGO = Transform.Find("Mode text").gameObject;
            _versionGO = Transform.Find("Version").gameObject;
            _leaderboardsGO = Transform.Find("Leaderboards").gameObject;
            _controlsGO = Transform.Find("Controls").gameObject;
            _tipsGO = Transform.Find("Tips").gameObject;

            _psychoModeTMP = _psychoModeGO.GetComponent<TextMeshPro>();
            _versionTMP = _versionGO.GetComponent<TextMeshPro>();
            _leaderboardsTMP = _leaderboardsGO.GetComponent<TextMeshPro>();
            _controlsTMP = _controlsGO.GetComponent<TextMeshPro>();
            _tipsTMP = _tipsGO.GetComponent<TextMeshPro>();

            _versionTMP.text = Translator.GetString("main_menu_27", Application.version);
            _leaderboardsTMP.text = Translator.GetString("main_menu_28");

            _controlsTMP.text = Translator.GetString("main_menu_29");

            SFX.MusicVolumeScale = 2f;
            UpdateTip();
        }

        protected override void Open()
        {
            base.Open();

            Global.OnUpdate += OnUpdate;
            _logoDrawer.ColliderEnabled = true;
            _langDrawer.ColliderEnabled = true;

            _flickeringTween.Kill();
            _flickeringTween = DOVirtual.Float(0, 0, 120000, v => OnFlickeringTweenUpdate()).SetUpdate(UpdateType.Fixed);
            _tipsTween.Kill();
            _tipsTween = DOVirtual.DelayedCall(10, OnTipsTweenComplete).SetLoops(-1, LoopType.Restart);
            if (_firstTimeOpened)
                _tipsTween.SetDelay(6);
            _firstTimeOpened = false;
        }
        protected override void Close()
        {
            base.Close();
            Global.OnUpdate -= OnUpdate;
            _flickeringTween.Kill();
            _tipsTween.Kill();
        }

        void StartTheGame()
        {
            Global.OnUpdate -= OnUpdate;
            _logoDrawer.ColliderEnabled = false;
            _langDrawer.ColliderEnabled = false;
            DOVirtual.Float(2, 1, 2, v => SFX.MusicVolumeScale = v);
            _ = Player.StartTheGame(this);
        }
        void UpdateTip()
        {
            int indexOfIndex = _tipsIndecies.RandomIndexSafe();
            int index = _tipsIndecies[indexOfIndex];
            _tipsIndecies.RemoveAt(indexOfIndex);
            if (_tipsIndecies.Count == 0)
                _tipsIndecies = Enumerable.Range(0, _tips.Length).ToList();
            _tipsTMP.text = _tips[index];
        }

        void OnFlickeringTweenUpdate()
        {
            int value = Utils.RandomIntSafe(0, 400);
            _controlsGO.SetActive(value != 2);
            _tipsGO.SetActive(value != 3);
        }
        void OnTipsTweenComplete()
        {
            UpdateTip();
        }
        void OnUpdate()
        {
            OnUpdate_UserModeSelect();
            OnUpdate_LogoColorIntensity();
        }

        void OnUpdate_UserModeSelect()
        {
            if (TableConsole.IsVisible) return;

            if (Input.GetKeyDown(KeyCode.E))
                PlayerConfig.chaosMode = !PlayerConfig.chaosMode;
            if (Input.GetKeyDown(KeyCode.Q))
                PlayerConfig.psychoMode = !PlayerConfig.psychoMode;

            if (PlayerConfig.psychoMode && PlayerConfig.chaosMode)
            {
                _psychoModeTMP.color = Color.magenta;
                _psychoModeTMP.text = Translator.GetString("main_menu_30");
            }
            else if (PlayerConfig.psychoMode)
            {
                _psychoModeTMP.color = Color.red;
                _psychoModeTMP.text = Translator.GetString("main_menu_31");
            }
            else if (PlayerConfig.chaosMode)
            {
                _psychoModeTMP.color = Color.cyan;
                _psychoModeTMP.text = Translator.GetString("main_menu_32");
            }

            bool isAnyMode = PlayerConfig.psychoMode || PlayerConfig.chaosMode;
            _logoInitialColor = isAnyMode ? _psychoModeTMP.color : Color.white;
            _logoDrawer.Color = _logoInitialColor;
            _psychoModeGO.SetActive(isAnyMode);
        }
        void OnUpdate_LogoColorIntensity()
        {
            float intensity = SFX.SpectrumVolume.ClampedMin(1);
            intensity = Mathf.Lerp(_logoLastIntensity, intensity, Time.deltaTime * 8);

            Color endColor = _logoInitialColor;
            endColor.r *= intensity;
            endColor.g *= intensity;
            endColor.b *= intensity;
            endColor.a = 1f;

            _logoDrawer.Color = endColor;
            _logoLastIntensity = intensity;
        }
    }
}
