#define DEMO // demo version code

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Environment;
using Game.Palette;
using Game.Sleeves;
using Game.Territories;
using GreenOne;
using MyBox;
using System;
using TMPro;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Меню для одного из игровых мест локации.
    /// </summary>
    public sealed class BattlePlaceMenu : PlaceMenu, IMenuWithTerritory
    {
        public TableTerritory Territory => _territory;

        protected override string PlaceId => "battle";
        protected override string HeaderText => string.Empty;
        protected override string LeftText => "Модификатор сражения:\nотсутствует";
        protected override string RightText => "Статус противника:\nобычный";

        static BattlePlaceMenu _instance;
        static readonly GameObject _prefab = Resources.Load<GameObject>("Prefabs/Menus/Places/Battle");

        #if DEMO
        const int DEMO_DIFFICULTY_MIN = 1;
        const int DEMO_DIFFICULTY_MAX = 5;

        static int _demoLocStageForPlayer;
        static int _demoLocStageForEnemy;
        static int _demoDifficulty = 0;
        static readonly Color[][] _demoPalettes = new Color[][]
        {
            new Color[]
            {
                Utils.HexToColor("#f5e6e8"),
                Utils.HexToColor("#aaa1c8"),
                Utils.HexToColor("#967aa1"),
                Utils.HexToColor("#192a51"),
                Utils.HexToColor("#08081f"),

                Utils.HexToColor("#00ffff"),
                Utils.HexToColor("#ffff00"),
            },
            new Color[]
            {
                Utils.HexToColor("#ebf4f6"),
                Utils.HexToColor("#37b7c3"),
                Utils.HexToColor("#088395"),
                Utils.HexToColor("#071952"),
                Utils.HexToColor("#08081f"),

                Utils.HexToColor("#00ffff"),
                Utils.HexToColor("#ffff00"),
            },
            new Color[]
            {
                Utils.HexToColor("#eff6e0"),
                Utils.HexToColor("#aec3b0"),
                Utils.HexToColor("#598392"),
                Utils.HexToColor("#124559"),
                Utils.HexToColor("#01161e"),

                Utils.HexToColor("#00ffff"),
                Utils.HexToColor("#ffff00"),
            },
            new Color[]
            {
                Utils.HexToColor("#edf2f4"),
                Utils.HexToColor("#ef233c"),
                Utils.HexToColor("#8d99ae"),
                Utils.HexToColor("#2b2d42"),
                Utils.HexToColor("#080808"),

                Utils.HexToColor("#ef233c"),
                Utils.HexToColor("#ffff00"),
            },
            new Color[]
            {
                Utils.HexToColor("#eeeeee"),
                Utils.HexToColor("#ff204e"),
                Utils.HexToColor("#5d0e41"),
                Utils.HexToColor("#00224d"),
                Utils.HexToColor("#08081f"),

                Utils.HexToColor("#ff204e"),
                Utils.HexToColor("#ffff00"),
            },
        };
        #endif

        readonly Drawer _turnButton;
        readonly Drawer _fleeButton;

        readonly TextMeshPro _logTextMesh;
        readonly TextMeshPro _descTextMesh;

        BattleSide _player;
        BattleSide _enemy;
        pTerritory _territory;

        class pFieldCard : BattleFieldCard, IBattleSleeveCard
        {
            public BattleSleeve Sleeve => Side.Sleeve;
            public ITableSleeveCard AsInSleeve => this;

            bool ITableSleeveCard.IsInMove { get => _isPulling; set => _isPulling = value; }
            bool ITableSleeveCard.IsPulledOut { get => _isPulledOut; set => _isPulledOut = value; }

            bool _isPulling;
            bool _isPulledOut;

            public pFieldCard(FieldCard data, BattleSide side, bool withDrawer = true) : base(data, side, withDrawer)
            {
                OnPostKilled.Add(OnPostKilledBase);
            }
            protected pFieldCard(pFieldCard src, BattleFieldCardCloneArgs args) : base(src, args) 
            {
                args.TryOnClonedAction(src.GetType(), typeof(pFieldCard));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleFieldCardCloneArgs cArgs)
                     return new pFieldCard(this, cArgs);
                else return null;
            }
            protected override TableCardDrawer DrawerCreator(Transform parent)
            {
                TableCardDrawer drawer = base.DrawerCreator(parent);
                drawer.OnMouseEnter += OnDrawerMouseEnter;
                drawer.OnMouseLeave += OnDrawerMouseLeave;
                drawer.OnMouseClickLeft += OnDrawerMouseClickLeft;
                return drawer;
            }

            bool ITableSleeveCard.CanTake()
            {
                bool result = false;
                if (Side.isMe && _instance._territory.PhaseAwaitsPlayer)
                    result = Side.CanAfford(this);

                if (!result)
                    Drawer.transform.DOAShake();
                return result;
            }
            bool ITableSleeveCard.CanReturn()
            {
                return true;
            }
            bool ITableSleeveCard.CanDropOn(TableField field)
            {
                if (field is BattleField bField)
                     return field.Card == null && bField.Side == Side;
                else return false;
            }

            bool ITableSleeveCard.CanPullOut()
            {
                return Side.CanAfford(this);
            }
            bool ITableSleeveCard.CanPullIn()
            {
                return true;
            }

            void ITableSleeveCard.Take()
            {
                AsInSleeve.TakeBase();
                if (Side.isMe) _instance.SetPlayerControls(false, andCardsColliders: true);
                SetFieldsHighlight(true);
            }
            void ITableSleeveCard.Return()
            {
                AsInSleeve.ReturnBase();
                if (Side.isMe) _instance.SetPlayerControls(true);
                SetFieldsHighlight(false);
            }
            void ITableSleeveCard.DropOn(TableField field)
            {
                AsInSleeve.DropOnBase();
                if (Side.isMe)
                {
                    _instance.SetPlayerControls(true, andCardsColliders: true);
                    Drawer.IgnoreFirstMouseEnter = true;
                }
                SetFieldsHighlight(false);

                Side.Purchase(this);
                Drawer.SetSortingOrder(0);
                Drawer.ChangePointer = false;

                Territory.PlaceFieldCard(this, (BattleField)field, Side);
            }

            void SetFieldsHighlight(bool value)
            {
                foreach (BattleField field in Side.Fields().WithoutCard())
                    field.Drawer.SetHighlight(value);
            }
            UniTask OnPostKilledBase(object sender, ITableEntrySource source)
            {
                pFieldCard card = (pFieldCard)sender;
                return card.Side.ether.AdjustValue(1, _instance);
            }

            void OnDrawerMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)sender;

                if (Sleeve.Contains(this) && !Side.CanAfford(this))
                    drawer.upperLeftIcon.RedrawChunksColor(Color.red);
            }
            void OnDrawerMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)sender;
                drawer.upperLeftIcon.RedrawChunksColor();
            }
            void OnDrawerMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)sender;

                if (drawer.attached.Field != null)
                    drawer.transform.DOAShake();
            }
        }
        class pFloatCard : BattleFloatCard, IBattleSleeveCard
        {
            public BattleSleeve Sleeve => Side.Sleeve;
            public ITableSleeveCard AsInSleeve => this;

            bool ITableSleeveCard.IsInMove { get => _isPulling; set => _isPulling = value; }
            bool ITableSleeveCard.IsPulledOut { get => _isPulledOut; set => _isPulledOut = value; }

            bool _isPulling;
            bool _isPulledOut;

            public pFloatCard(FloatCard data, BattleSide side, bool withDrawer = true) : base(data, side, withDrawer) { }
            protected pFloatCard(pFloatCard src, BattleFloatCardCloneArgs args) : base(src, args) 
            {
                args.TryOnClonedAction(src.GetType(), typeof(pFloatCard));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleFloatCardCloneArgs cArgs)
                    return new pFloatCard(this, cArgs);
                else return null;
            }
            protected override TableCardDrawer DrawerCreator(Transform parent)
            {
                TableCardDrawer drawer = base.DrawerCreator(parent);
                drawer.OnMouseEnter += OnDrawerMouseEnter;
                drawer.OnMouseLeave += OnDrawerMouseLeave;
                return drawer;
            }
            protected override async UniTask OnUsed(TableFloatCardUseArgs e)
            {
                await base.OnUsed(e);
                if (Side.isMe)
                    _instance.SetPlayerControls(true, andCardsColliders: true);
            }

            bool ITableSleeveCard.CanTake()
            {
                bool result = false;
                if (Side.isMe && _instance._territory.PhaseAwaitsPlayer)
                    result = Side.CanAfford(this);

                if (!result)
                    Drawer.transform.DOAShake();
                return result;
            }
            bool ITableSleeveCard.CanReturn()
            {
                return true;
            }
            bool ITableSleeveCard.CanDropOn(TableField field)
            {
                return Data.IsUsable(new TableFloatCardUseArgs(this, Territory));
            }

            bool ITableSleeveCard.CanPullOut()
            {
                return Side.CanAfford(this);
            }
            bool ITableSleeveCard.CanPullIn()
            {
                return true;
            }

            void ITableSleeveCard.Take()
            {
                AsInSleeve.TakeBase();
                if (Side.isMe) _instance.SetPlayerControls(false, andCardsColliders: true);
            }
            void ITableSleeveCard.Return()
            {
                AsInSleeve.ReturnBase();
                if (Side.isMe) _instance.SetPlayerControls(true);
            }
            void ITableSleeveCard.DropOn(TableField field)
            {
                AsInSleeve.DropOnBase();
                Side.Purchase(this);
                Territory.PlaceFloatCard(this, Side);
            }

            void OnDrawerMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                BattleFloatCardDrawer drawer = (BattleFloatCardDrawer)sender;

                if (Sleeve.Contains(this) && !Side.CanAfford(this))
                    drawer.upperLeftIcon.RedrawChunksColor(Color.red);
            }
            void OnDrawerMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                if (e.handled) return;
                BattleFloatCardDrawer drawer = (BattleFloatCardDrawer)sender;
                drawer.upperLeftIcon.RedrawChunksColor();
            }
        }
        class pSleeve : BattleSleeve
        {
            public pSleeve(BattleSide side, Transform parent, bool withDrawer = true) : base(side, parent, withDrawer) { }
            protected pSleeve(pSleeve src, BattleSleeveCloneArgs args) : base(src, args) 
            {
                foreach (ITableSleeveCard card in src)
                    Add(HoldingCardCloner(card, args));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleSleeveCloneArgs cArgs)
                     return new pSleeve(this, cArgs);
                else return null;
            }

            protected override ITableSleeveCard HoldingCardCreator(Card data)
            {
                ITableSleeveCard card;
                bool withDrawer = Drawer != null;

                if (data.isField)
                     card = new pFieldCard((FieldCard)data, Side, withDrawer);
                else card = new pFloatCard((FloatCard)data, Side, withDrawer);

                //if (!isForMe)
                //    card.Drawer.Flip();

                return card;
            }
            protected override ITableSleeveCard HoldingCardCloner(ITableSleeveCard src, TableSleeveCloneArgs args)
            {
                BattleSleeveCloneArgs argsCast = (BattleSleeveCloneArgs)args;
                Card dataClone = (Card)src.Data.Clone();

                ITableSleeveCard cardClone;
                if (dataClone.isField)
                {
                    BattleFieldCardCloneArgs cardCArgs = new((FieldCard)dataClone, null, Side, argsCast.terrCArgs);
                    cardClone = (pFieldCard)src.Clone(cardCArgs);
                }
                else
                {
                    BattleFloatCardCloneArgs cardCArgs = new((FloatCard)dataClone, Side, argsCast.terrCArgs);
                    cardClone = (pFloatCard)src.Clone(cardCArgs);
                }
                return cardClone;
            }
        }
        class pSide : BattleSide
        {
            public override int HealthAtStart => (isMe ? _demoLocStageForPlayer : _demoLocStageForEnemy) * 2;

            public pSide(BattleTerritory territory, bool isMe) : base(territory, isMe) { }
            protected pSide(pSide src, BattleSideCloneArgs args) : base(src, args) { }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleSideCloneArgs cArgs)
                    return new pSide(this, cArgs);
                else return null;
            }

            protected override CardDeck DeckCreator()
            {
                Traveler.Location.stage = isMe ? _demoLocStageForPlayer : _demoLocStageForEnemy;
                if (isMe)
                     return (CardDeck)Player.Deck.Clone();
                else return Traveler.NewDeck();
            }
            protected override BattleSleeve SleeveCreator(Transform parent)
            {
                return new pSleeve(this, parent, Drawer != null);
            }
        }
        class pTerritory : BattleTerritory
        {
            bool _battleStartedLogWritten;

            public pTerritory(bool playerMovesFirst, int sizeX) : base(playerMovesFirst, sizeX, _instance.Transform) { }
            protected pTerritory(pTerritory src, BattleTerritoryCloneArgs args) : base(src, args)
            {
                args.TryOnClonedAction(src.GetType(), typeof(pTerritory));
            }
            public override object Clone(CloneArgs args)
            {
                if (args is BattleTerritoryCloneArgs cArgs)
                     return new pTerritory(this, cArgs);
                else return null;
            }

            protected override BattleFieldCard FieldCardCreator(FieldCard data, BattleSide side)
            {
                return new pFieldCard(data, side, !DrawersAreNull);
            }
            protected override BattleFloatCard FloatCardCreator(FloatCard data, BattleSide side)
            {
                return new pFloatCard(data, side, !DrawersAreNull);
            }

            protected override BattleSide PlayerSideCreator()
            {
                return new pSide(territory: this, isMe: true);
            }
            protected override BattleSide EnemySideCreator()
            {
                return new pSide(territory: this, isMe: false);
            }

            // NOTE: 'terr.DrawersAreNull' check is required to determine either this territory is a clone for AI (without drawers) or not (not the best solution)
            protected override async UniTask OnStartPhaseBase(object sender, EventArgs e)
            {
                await base.OnStartPhaseBase(sender, e);
                pTerritory terr = (pTerritory)sender;

                if (terr.DrawersAreNull)
                    terr.NextPhase();
                else VFX.CreateText($"ХОД {terr.Turn}", Color.white, Vector3.zero).DOATextPopUp(0.5f, onComplete: () => terr.NextPhase());
            }
            protected override async UniTask OnEnemyPhaseBase(object sender, EventArgs e)
            {
                await base.OnEnemyPhaseBase(sender, e);
                pTerritory terr = (pTerritory)sender;
                SidePlacePhase(terr, terr.enemy);
            }
            protected override async UniTask OnPlayerPhaseBase(object sender, EventArgs e)
            {
                await base.OnPlayerPhaseBase(sender, e);
                pTerritory terr = (pTerritory)sender;
                SidePlacePhase(terr, terr.player);
            }
            protected override async UniTask OnEndPhaseBase(object sender, EventArgs e)
            {
                await base.OnEndPhaseBase(sender, e);
            }
            protected override async UniTask OnNextPhaseBase(object sender, EventArgs e)
            {
                await base.OnNextPhaseBase(sender, e);
                if (_battleStartedLogWritten) return;
                Menu.WriteLogToCurrent("-- НАЧАЛО БОЯ -- ");
                _battleStartedLogWritten = true;
            }
            protected override async UniTask OnPlayerWonBase(object sender, EventArgs e)
            {
                await base.OnPlayerWonBase(sender, e);
                pTerritory terr = (pTerritory)sender;
                if (terr.DrawersAreNull) return;

                _instance.OnPlayerWon(sender, e);
                Menu.WriteLogToCurrent("-- КОНЕЦ БОЯ -- ");
                Menu.WriteLogToCurrent("-- ИГРОК ПОБЕДИЛ -- ");
            }
            protected override async UniTask OnPlayerLostBase(object sender, EventArgs e)
            {
                await base.OnPlayerLostBase(sender, e);
                pTerritory terr = (pTerritory)sender;
                if (terr.DrawersAreNull) return;

                _instance.OnPlayerLost(sender, e);
                Menu.WriteLogToCurrent("-- КОНЕЦ БОЯ -- ");
                Menu.WriteLogToCurrent("-- ИГРОК ПРОИГРАЛ -- ");
            }

            protected override void OnInitiationsProcessingStartBase(object sender, EventArgs e)
            {
                base.OnInitiationsProcessingStartBase(sender, e);
                BattleInitiationQueue queue = (BattleInitiationQueue)sender;
                BattleTerritory terr = queue.Territory;
                if (!terr.DrawersAreNull)
                    _instance.SetPlayerControls(false);
            }
            protected override void OnInitiationsProcessingEndBase(object sender, EventArgs e)
            {
                base.OnInitiationsProcessingEndBase(sender, e);
                BattleInitiationQueue queue = (BattleInitiationQueue)sender;
                BattleTerritory terr = queue.Territory;
                if (!terr.DrawersAreNull)
                    _instance.SetPlayerControls(PhaseAwaitsPlayer);
            }

            async UniTask SidePlacePhase(pTerritory sender, BattleSide side)
            {
                // 1. avoids recursion (from side.ai.TryMakeTurn(), it calls PhaseBase events)
                // 2. does not adjust gold and take cards as this method can be called recursively (and virtually) only from other side
                //    (if this side moves first) to save some memory and CPU calculations
                if (sender.DrawersAreNull) return;

                side.gold.AdjustValue(1, _instance);
                await side.Sleeve.TakeMissingCards();

                if (!side.isMe)
                     side.ai.MakeTurn();
                else _instance.SetPlayerControls(true);
            }
        }

        public BattlePlaceMenu() : base("battle", UIFlags.WithNothing, _prefab)
        {
            _turnButton = new Drawer(null, Transform.Find<SpriteRenderer>("Turn button"));
            _fleeButton = new Drawer(null, Transform.Find<SpriteRenderer>("Flee button"));

            _logTextMesh = Transform.Find<TextMeshPro>("Log window/Text");
            _descTextMesh = Transform.Find<TextMeshPro>("Desc window/Text");

            _turnButton.OnMouseClickLeft += (s, e) => TryEndTurn();
            _turnButton.Tooltip = () => $"Завершить ход {_territory.Turn}.";

            _fleeButton.OnMouseClickLeft += (s, e) => TryFlee();
            _fleeButton.Tooltip = () => "Шанс побега: 100%.";

            _logTextMesh.text = Log;
            _descTextMesh.text = "";

            SetBellState(false);
            SetFleeState(false);
        }
        public override void OpenInstantly()
        {
            base.OpenInstantly();
            #if DEMO
            demo_OnBattleStart(_demoDifficulty == 0);
            #else
            _instance = this;
            _territory = new pTerritory(playerMovesFirst: true /*UnityEngine.Random.value > 0.5f*/, sizeX: 5); // TODO: replace
            _player = _territory.player;
            _enemy = _territory.enemy;
            _territory.NextPhase();
            #endif
        }

        // TODO: implement normally
        public override UniTask OpenAnimated()
        {
            return OpenAnimatedBase(withColliders: false);
        }
        public override UniTask CloseAnimated()
        {
            return CloseAnimatedBase(withColliders: false);

            //MusicPack.Get("Battle").StopFading();
            //BattleArea.StopTargetAiming();

            //Player.Gold += _territory.player.gold;
            //Player.HealthCurrent = _territory.player.health;

            //if (_playerWon)
            //     Menu.Get("Location").OpenInstantly();
            //else Menu.Get("World").OpenInstantly();
        }

        public override void SetColliders(bool value)
        {
            base.SetColliders(value);
            SetPlayerControls(value);
            _territory.SetColliders(value);
        }
        public override void WriteLog(string text)
        {
            base.WriteLog(text);
            _logTextMesh.text = Log;
        }
        public override void WriteDesc(string text)
        {
            base.WriteDesc(text);
            _descTextMesh.text = text;
        }

        async void TryFlee()
        {
            OnPlayerWon(null, null); // TODO: remove
            //SetPlayerControls(false);
            //SetFleeState(false);

            //await VFX.CreateScreenBG(Color.clear, Transform).DOFade(1, 0.5f).AsyncWaitForCompletion();
            //VFX.CreateText("СБЕЖАЛ...", Color.gray, Transform).transform.DOAShake();
            //await UniTask.Delay(2000);
            //Application.Quit();
        }
        async void TryEndTurn()
        {
            SetPlayerControls(false);
            _territory.NextPhase();
        }

        async void OnPlayerWon(object sender, EventArgs e)
        {
            SetColliders(false);
            await UniTask.Delay(1000);
            #if DEMO
            demo_OnBattleEnd();
            #else
            await CloseAnimated();
            #endif
        }
        async void OnPlayerLost(object sender, EventArgs e)
        {
            SetColliders(false);
            Traveler.TryStopTravel(complete: false);
            await VFX.CreateScreenBG(Color.clear, Transform).DOFade(1, 0.5f).AsyncWaitForCompletion();
            VFX.CreateText("КОНЕЦ", Color.red, Transform).transform.DOAShake();
            await UniTask.Delay(2000);
            await CloseAnimated();
        }

        void SetPlayerControls(bool value, bool andCardsColliders = false)
        {
            if (andCardsColliders)
            {
                foreach (BattleField field in _territory.Fields().WithCard())
                    field.Drawer.SetCollider(value);
            }

            SetBellState(value);
            SetFleeState(value);
            _player.Sleeve.Drawer.CanPullOut = value;
            _enemy.Sleeve.Drawer.CanPullOut = value;
        }

        void SetBellState(bool value)
        {
            _turnButton.SetCollider(value);
            _turnButton.gameObject.GetComponent<SpriteRenderer>().color = value ? Color.white : Color.white.WithAlpha(0.5f);
        }
        void SetFleeState(bool value)
        {
            _fleeButton.SetCollider(value);
            _fleeButton.gameObject.GetComponent<SpriteRenderer>().color = value ? Color.white : Color.white.WithAlpha(0.5f);
        }

        #if DEMO
        Menu demo_CreateCardChooseAndUpgrade(int cPointsPerCard, int cFieldChoices, int cFloatChoices, int cCardsPerChoice, int uPointsLimit)
        {
            CardChooseMenu menu = new(cPointsPerCard, cFieldChoices, cFloatChoices, cCardsPerChoice, cFieldChoices / 2);
            menu.MenuWhenClosed = () => demo_CreateCardUpgrade(uPointsLimit);
            menu.OnClosed += menu.DestroyInstantly;
            return menu;
        }
        Menu demo_CreateCardUpgrade(int uPointsLimit)
        {
            CardUpgradeMenu menu = new(Player.Deck, uPointsLimit);
            menu.MenuWhenClosed = () => this;
            menu.OnClosed += menu.DestroyInstantly;
            return menu;
        }

        async void demo_OnBattleEnd()
        {
            if (_demoDifficulty == DEMO_DIFFICULTY_MAX)
            {
                await VFX.CreateScreenBG(Color.clear, Transform).DOFade(1, 0.5f).AsyncWaitForCompletion();
                VFX.CreateText("ФИНАЛ", Color.white, Transform).transform.DOAShake();
                await UniTask.Delay(2000);
                await CloseAnimated();
                return;
            }

            int pointsPerCard = demo_DifficultyToLocStage(_demoDifficulty + 1);
            Traveler.DeckCardsCount(pointsPerCard, out int fieldCards, out int floatCards);
            int pointsLimit = fieldCards * pointsPerCard;

            int fieldsChoices = (fieldCards - Player.Deck.fieldCards.Count).ClampedMin(1);
            int floatsChoices = (floatCards - Player.Deck.floatCards.Count).ClampedMin(0);
            int cardsPerChoice = (_demoDifficulty + 2).ClampedMax(5);

            Menu menu = demo_CreateCardChooseAndUpgrade(pointsPerCard, fieldsChoices, floatsChoices, cardsPerChoice, pointsLimit);
            MenuTransit.Between(this, menu);
        }
        async void demo_OnBattleStart(bool firstOpening)
        {
            ColorPalette.SetPalette(_demoPalettes[_demoDifficulty]);
            _demoDifficulty++;

            int locStage = demo_DifficultyToLocStage(_demoDifficulty);
            float locStageDifficultyScale = demo_DifficultyToLocStageScale(_demoDifficulty);

            _demoLocStageForPlayer = locStage;
            _demoLocStageForEnemy = (locStage * locStageDifficultyScale).Ceiling();

            _instance = this;
            _territory?.Dispose();
            _territory = new pTerritory(playerMovesFirst: true /*UnityEngine.Random.value > 0.5f*/, sizeX: 5); // TODO: replace
            _player = _territory.player;
            _enemy = _territory.enemy;
            _enemy.ai.Style = (BattleAI.PlayStyle)UnityEngine.Random.Range(0, 3);

            SpriteRenderer bg = VFX.CreateScreenBG(firstOpening ? Color.black : Color.clear);
            await UniTask.Delay(500);
            string text1 = $"ЭТАП {_demoDifficulty}/{DEMO_DIFFICULTY_MAX}";
            string text2Hex = ColorPalette.GetColorInfo(1).Hex;
            string text2 = $"\n<size=50%><color={text2Hex}>угроза: {_demoLocStageForPlayer}    сложность: {locStageDifficultyScale * 100}%";
 
            TextMeshPro bgText = VFX.CreateText(text1, Color.white, bg.transform);
            bgText.transform.position = Vector2.up * 136;
            bgText.transform.DOAShake();
            bgText.richText = true;
            bgText.sortingOrder = bg.sortingOrder + 1;
            bgText.alignment = TextAlignmentOptions.Top;

            await UniTask.Delay(500);
            bgText.transform.DOAShake();
            bgText.text += text2;

            await UniTask.Delay(2000);
            bg.DOFade(0, 0.5f);
            await bgText.DOFade(0, 0.5f).SetDelay(1f).AsyncWaitForCompletion();

            bg.gameObject.Destroy();
            _territory.NextPhase();
        }

        static float demo_DifficultyToLocStageScale(int difficulty) => difficulty switch
        {
            1 => 0.88f,
            2 => 1.00f,
            3 => 1.12f,
            4 => 1.24f,
            5 => 1.36f,
            _ => throw new NotSupportedException(),
        };
        static int demo_DifficultyToLocStage(int difficulty) => difficulty switch
        {
            1 => 8,
            2 => 16,
            3 => 32,
            4 => 64,
            5 => 128,
            _ => throw new NotSupportedException(),
        };
        #endif
    }
}
