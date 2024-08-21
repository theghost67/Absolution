using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Environment;
using Game.Palette;
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
        static int _tipsIndex;
        static bool _firstTimeOpened;

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
            }

            protected override void SetColor(Color value)
            {
                base.SetColor(value);
                _renderer.color = value;
            }

            protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseEnterBase(sender, e);
                _scaleTween.Kill();
                _scaleTween = transform.DOScale(1.1f, 0.33f).SetEase(Ease.OutQuad);
            }
            protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseLeaveBase(sender, e);
                _scaleTween.Kill();
                _scaleTween = transform.DOScale(1.00f, 0.33f).SetEase(Ease.OutQuad);
            }
            protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
            {
                base.OnMouseClickBase(sender, e);
                _menu.StartTheGame();
            }
        }

        static MainMenu() 
        {
            _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
            _tips = new string[]
            {
                "Не стоит использовать перемотку, если вы начинающий игрок. Так можно и проморгать важные моменты.",
				"Карты способностей имеют мощные эффекты, полезные на любом этапе сражения.",
                "Высокая стоимость или низкая инициатива карты облегчает её улучшение. Так же работает и в обратную сторону.",
                "Вы всегда ходите первым на нечётных этапах, но зато ходите последним на чётных. P.S.: последним ходить лучше.",
                "Маленький размер колоды позволяет создать мощные карты. Однако, это довольно рискованная тактика, так как она позволит противнику изнеможить эти карты.",
                "Редкость навыка зачастую определяет сложность его использования, а не общую силу. Хотя, в умелых руках они становятся действительно сильными.",
                "Не знаете, как победить мощную карту противника? Что ж, запасайтесь сосисками и прицельными навыками.",
                "Навыки перемещения карт позволяют оперативно защищаться или атаковать, не тратя при этом никаких ресурсов.",
                "Часто, победа наступает тогда, когда у противника заканчиваются карты в руке. Война на истощение? Да, похоже на то.",
                "Слишком легко? Попробуйте пройти игру с режимом психа, не улучшать карты или ограничить максимум карт в колоде. Только не надо делать всё сразу...",
                "Слишком сложно? Попробуйте найти мощную синергию навыков или использовать \"подлые\" подходы, по типу прицельного фокуса поля или особых карт способностей.",
                "Затишье перед бурей никогда не приводит к чему-либо хорошему.",
                "На первом ходу лучшим решением будет защитить все свои поля картами.",
                "Победа присуждается только в начале хода. Если и вы, и противник будете мертвы к началу хода - вы всё равно проиграете.",
                "Пройти хардкорный режим далеко не так просто. Только игроки с превосходной колодой, знанием механик игры и каплей удачи могут бросить ему вызов.",
				"Ускорение сражения не влияет на таймер прохождения. Готовы устроить спидран?",
            };
            _tipsIndex = Random.Range(0, _tips.Length);
            _firstTimeOpened = true;
        }
        public MainMenu() : base(ID, _prefab)
        {
            _logoDrawer = new LogoDrawer(this);

            _psychoModeGO = Transform.Find("Psycho mode").gameObject;
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
            _tipsTMP.text = _tips[_tipsIndex];
            SFX.MusicVolumeScale = 2f;
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
        public override void OnTransitMiddle(bool isFromThis)
        {
            base.OnTransitMiddle(isFromThis);
            if (isFromThis) return;
            for (int i = 0; i < ColorPalette.All.Length; i++)
                ColorPalette.Current = Global.Palette;
        }

        async void StartTheGame()
        {
            Global.OnUpdate -= OnUpdate;
            _logoDrawer.ColliderEnabled = false;

            Location location = EnvironmentBrowser.Locations["college"];
            CardChooseMenu menu = new(location.stage, 4, 0, 3, PlayerConfig.rerollsAtStart);
            menu.MenuWhenClosed = () => new BattlePlaceMenu();
            menu.OnClosed += menu.TryDestroy;

            await UniTask.Delay(1000);

            Player.Deck.Clear();
            DOVirtual.Float(2, 1, 2, v => SFX.MusicVolumeScale = v);
            AudioBrowser.Shuffle();

            _ = Traveler.TryStartTravel(new LocationMission(location));
            _ = MenuTransit.Between(this, menu);
        }

        void OnFlickeringTweenUpdate()
        {
            int value = Random.Range(0, 200);
            _controlsGO.SetActive(value != 2);
            _tipsGO.SetActive(value != 3);
        }
        void OnTipsTweenComplete()
        {
            _tipsIndex = ++_tipsIndex % _tips.Length;
            _tipsTMP.text = _tips[_tipsIndex];
        }
        void OnUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.Q)) return;
            PlayerConfig.psychoMode = !PlayerConfig.psychoMode;
            _logoDrawer.Color = PlayerConfig.psychoMode ? Color.red : Color.white;
            _psychoModeGO.SetActive(PlayerConfig.psychoMode);
        }
    }
}
