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

        static MainMenu() 
        {
            _logoLastIntensity = 1;
            _logoInitialColor = Color.white;
            _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
            _tips = new string[]
            {
                "Только начинаете играть? Не используйте переброс карт или навыков, играйте как ляжет рука. Также, вы можете игнорировать навыки противника - вы узнаете о них в процессе игры.",
                "Не стоит использовать перемотку, если вы начинающий игрок. Так можно и проморгать важные моменты.",
				"Карты способностей имеют мощные эффекты, полезные на любом этапе сражения. Всегда неплохо захватить с собой парочку.",
                "Высокая стоимость или низкая инициатива карты облегчает её улучшение. Так же работает и в обратную сторону.",
                "Вы всегда ходите первым на нечётных этапах, но зато ходите последним на чётных. P.S.: последним ходить лучше.",
                "Маленький размер колоды позволяет создать мощные карты. Однако, это довольно рискованная тактика, так как она позволит противнику изнеможить эти карты.",
                "Редкость навыка зачастую определяет сложность его использования, а не общую силу. Хотя, в умелых руках они становятся действительно сильными.",
                "Не знаете, как победить мощную карту противника? Что ж, запасайтесь сосисками и прицельными навыками. Сказал бы я в версии 1.3...",
                "Навыки перемещения карт позволяют оперативно защищаться или атаковать, не тратя при этом никаких ресурсов.",
                "Часто, победа наступает тогда, когда у противника заканчиваются карты в руке. Война на истощение? Да, похоже на то.",
                "Слишком легко? Попробуйте пройти игру с режимом психа, не улучшать карты или ограничить максимум карт в колоде. Только не надо делать всё сразу...",
                "Слишком сложно? Попробуйте найти мощную синергию навыков или использовать \"подлые\" подходы, по типу прицельной атаки одного поля или особых карт способностей.",
                "Затишье перед бурей никогда не приводит к чему-либо хорошему.",
                "На первом ходу лучшим решением будет защитить все свои поля картами.",
                "Победа присуждается только в начале хода. Если и вы, и противник будете мертвы к началу хода - вы всё равно проиграете.",
                "Пройти хардкорный режим далеко не так просто. Только игроки с превосходной колодой, знанием механик игры и каплей удачи могут бросить ему вызов.",
				"Ускорение сражения не влияет на таймер прохождения. Готовы устроить спидран?",
                "Не хватает золота или эфира? Что ж, некоторые карты помогут вам подзаработать. Однако, вряд ли эти карты будут мощными.",
                "Слышали про <color=#00FFFF>Метку создателя</color>? Только настоящие эксперты карточных сражений смогут собрать их все, не умерев по пути.",
                "Хотите новых ощущений? Или, может, просто захотелось посмеяться? Тогда добро пожаловать в режим хаоса! Нажмите \'E\', чтобы начать веселье.",
                "Если захотелось пощекотать нервы в режиме психа, вот вам один совет: навыки, восстанавливающие здоровье стороне-владельцу могут дать вам право на ошибку!",
                "Массовая атака несёт в себе большую угрозу. Представьте что будет, если усилить эту атаку?",
                "Размер колоды, так же, как и размер руки, ограничен шестнадцатью картами.",
                "В игре много контента, не пытайтесь изучить его весь сразу. Просто плывите по течению и наслаждайтесь внезапными событиями.",
                "Некоторые способности карт могут активироваться прямо в рукаве. Вот это фокусы!",
                "В отличие от вас, противник может улучшать свои карты перед первым раундом. Да уж, жизнь несправедлива."
            };
            _tipsIndecies = Enumerable.Range(0, _tips.Length).ToList();
            _firstTimeOpened = true;
        }
        public MainMenu() : base(ID, _prefab)
        {
            _logoDrawer = new LogoDrawer(this);

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

            _versionTMP.text = $"Версия игры: {Application.version}";
            SFX.MusicVolumeScale = 2f;
            UpdateTip();
        }

        protected override void Open()
        {
            base.Open();

            Global.OnUpdate += OnUpdate;
            _logoDrawer.ColliderEnabled = true;

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
            DOVirtual.Float(2, 1, 2, v => SFX.MusicVolumeScale = v);
            _ = Player.StartTheGame(this);
        }
        void UpdateTip()
        {
            int indexOfIndex = _tipsIndecies.GetRandomIndex();
            int index = _tipsIndecies[indexOfIndex];
            _tipsIndecies.RemoveAt(indexOfIndex);
            if (_tipsIndecies.Count == 0)
                _tipsIndecies = Enumerable.Range(0, _tips.Length).ToList();
            _tipsTMP.text = _tips[index];
        }

        void OnFlickeringTweenUpdate()
        {
            int value = UnityEngine.Random.Range(0, 400);
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
                _psychoModeTMP.text = "ФУЛЛ ХАОС: ПРИГОТОВЬТЕСЬ СТРАДАТЬ\nРЕЖИМ ПСИХА + РЕЖИМ ХАОСА.";
            }
            else if (PlayerConfig.psychoMode)
            {
                _psychoModeTMP.color = Color.red;
                _psychoModeTMP.text = "РЕЖИМ ПСИХА: У ВАС НЕТ ПРАВА НА ОШИБКУ\nМАКС. ЗДОРОВЬЕ ОГРАНИЧЕНО ДО 1 ЕД.";
            }
            else if (PlayerConfig.chaosMode)
            {
                _psychoModeTMP.color = Color.cyan;
                _psychoModeTMP.text = "РЕЖИМ ХАОСА: ДА НАЧНЁТСЯ ВЕСЕЛЬЕ!\nУ КАРТ ДОБАВЛЯЮТСЯ СЛУЧАЙНЫЕ НАВЫКИ.";
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
