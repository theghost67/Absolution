#define DEMO // demo version code

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Backgrounds;
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
using UnityEngine.Rendering.Universal;

namespace Game.Menus
{
    /// <summary>
    /// Меню для одного из игровых мест локации.
    /// </summary>
    public class BattlePlaceMenu : Menu, IMenuWithTerritory
    {
        const string ID = "battle";

        static readonly GameObject _prefab = Resources.Load<GameObject>("Prefabs/Menus/Places/Battle");
        public BattleTerritory Territory => _territory;
        public override string LinkedMusicMixId => ID;
        public int DemoDifficulty { get => _demoDifficulty; set => _demoDifficulty = value; }

        #if DEMO
        const int DEMO_DIFFICULTY_MIN = 1;
        const int DEMO_DIFFICULTY_MID = 5;
        const int DEMO_DIFFICULTY_MAX = 7;

        static int _demoLocStageForPlayer;
        static int _demoLocStageForEnemy;
        static int _demoDifficulty = 0;
        static float _demoDifficultyScale;
        static int _demoPrevFieldsCount;
        static int _demoPrevFloatsCount;

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
                Utils.HexToColor("#fff5e0"),
                Utils.HexToColor("#8decB4"),
                Utils.HexToColor("#41b06e"),
                Utils.HexToColor("#141e46"),
                Utils.HexToColor("#000000"),

                Utils.HexToColor("#00ffff"),
                Utils.HexToColor("#ffff00"),
            },
            new Color[]
            {
                Utils.HexToColor("#f5e6e8"),
                Utils.HexToColor("#c8acd6"),
                Utils.HexToColor("#433d8b"),
                Utils.HexToColor("#2e236c"),
                Utils.HexToColor("#17153b"),

                Utils.HexToColor("#00ffff"),
                Utils.HexToColor("#ffff00"),
            },
            new Color[]
            {
                Utils.HexToColor("#ffedd8"),
                Utils.HexToColor("#eabe6c"),
                Utils.HexToColor("#891652"),
                Utils.HexToColor("#240a34"),
                Utils.HexToColor("#000000"),

                Utils.HexToColor("#00ffff"),
                Utils.HexToColor("#eabe6c"),
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

        bool _territoryEndTurnIsPending;
        pTerritory _territory;
        SquaresBackground _bg;
        Tween _deathsDoorVolumeTween;
        Tween _deathsDoorPitchTween;
        Tween _beatTween;

        TableTerritory IMenuWithTerritory.Territory => _territory;

        protected class pFieldCard : BattleFieldCard, IBattleSleeveCard
        {
            public BattleSleeve Sleeve => Side.Sleeve;
            public ITableSleeveCard AsInSleeve => this;
            readonly BattlePlaceMenu _menu;
            bool _endTurnDropBlockIsActive; // will not allow to take/place cards after turn button was clicked

            public pFieldCard(BattlePlaceMenu menu, FieldCard data, BattleSide side) : base(data, side)
            {
                _menu = menu;
                TryOnInstantiatedAction(GetType(), typeof(pFieldCard));
            }
            protected pFieldCard(pFieldCard src, BattleFieldCardCloneArgs args) : base(src, args) 
            {
                TryOnInstantiatedAction(GetType(), typeof(pFieldCard));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleFieldCardCloneArgs cArgs)
                     return new pFieldCard(this, cArgs);
                else return null;
            }
            protected override Drawer DrawerCreator(Transform parent)
            {
                if (Territory.DrawersAreNull) return null;
                Drawer drawer = base.DrawerCreator(parent);
                drawer.OnMouseEnter += OnDrawerMouseEnter;
                drawer.OnMouseLeave += OnDrawerMouseLeave;
                drawer.OnMouseClick += OnDrawerMouseClickLeft;
                return drawer;
            }
            protected override async UniTask OnPostKilledBase_TOP(object sender, BattleKillAttemptArgs e)
            {
                await base.OnPostKilledBase_TOP(sender, e);
                pFieldCard card = (pFieldCard)sender;
                await card.Side.Ether.AdjustValue(1, _menu);
            }

            public bool CanTake()
            {
                if (BattleArea.IsAnyAiming) return false;
                bool result = Side.isMe && Side.CanAfford(this) && _menu._territory.IsPlayerPhase() && !_endTurnDropBlockIsActive;
                if (!result)
                    Drawer.transform.DOAShake();
                return result;
            }
            public bool CanReturn()
            {
                return true;
            }
            public bool CanDropOn(TableField field)
            {
                if (field is BattleField bField)
                     return field.Card == null && bField.Side == Side;
                else return false;
            }

            public bool CanPullOut()
            {
                return Side.isMe && Side.CanAfford(this);
            }
            public bool CanPullIn()
            {
                return true;
            }

            public void OnTake()
            {
                AsInSleeve.OnTakeBase();
                _menu._territory.SetCardsColliders(false);
                SetFieldsHighlight(true);
            }
            public void OnReturn()
            {
                AsInSleeve.OnReturnBase();
                _menu._territory.SetFieldsColliders(true);
                SetFieldsHighlight(false);
            }
            public void OnDropOn(TableSleeveCardDropArgs e)
            {
                AsInSleeve.OnDropOnBase(e);
                if (Side.isMe)
                {
                    _menu._territory.SetFieldsColliders(true);
                    Drawer.IsSelected = false;
                    SetFieldsHighlight(false);
                }
                if (Drawer.IsFlipped)
                    Drawer.FlipY();

                if (!e.isPreview)
                    _endTurnDropBlockIsActive = _menu._territoryEndTurnIsPending;
                else
                {
                    _endTurnDropBlockIsActive = false;
                    return;
                }

                Side.Purchase(this);
                Drawer.ChangePointer = false;
                _ = Territory.PlaceFieldCard(this, (BattleField)e.field, Side);
            }
            public Tween OnPullOut(bool sleevePull)
            {
                return AsInSleeve.OnPullOutBase(sleevePull);
            }
            public Tween OnPullIn(bool sleevePull)
            {
                return AsInSleeve.OnPullInBase(sleevePull);
            }

            void SetFieldsHighlight(bool value)
            {
                foreach (BattleField field in Side.Fields().WithoutCard())
                {
                    if (value) field.Drawer.ColliderEnabled = true;
                    field.Drawer.SetHighlight(value);
                }
            }
            void OnDrawerMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)sender;
                if (!Side.isMe) return;
                if (Sleeve.Contains(this) && !Side.CanAfford(this))
                    drawer.priceIcon.RedrawColor(Color.red);
            }
            void OnDrawerMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)sender;
                if (!Side.isMe) return;
                drawer.priceIcon.RedrawColor();
            }
            void OnDrawerMouseClickLeft(object sender, DrawerMouseEventArgs e)
            {
                BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)sender;

                if (drawer.attached.Field != null)
                    drawer.transform.DOAShake();
            }
        }
        protected class pFloatCard : BattleFloatCard, IBattleSleeveCard
        {
            public BattleSleeve Sleeve => Side.Sleeve;
            public ITableSleeveCard AsInSleeve => this;
            readonly BattlePlaceMenu _menu;
            bool _endTurnDropBlockIsActive; // will not allow to take/place cards after turn button was clicked

            public pFloatCard(BattlePlaceMenu menu, FloatCard data, BattleSide side) : base(data, side)
            { 
                _menu = menu;
                TryOnInstantiatedAction(GetType(), typeof(pFloatCard));
            }
            protected pFloatCard(pFloatCard src, BattleFloatCardCloneArgs args) : base(src, args) 
            {
                TryOnInstantiatedAction(GetType(), typeof(pFloatCard));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleFloatCardCloneArgs cArgs)
                    return new pFloatCard(this, cArgs);
                else return null;
            }
            protected override Drawer DrawerCreator(Transform parent)
            {
                if (Territory.DrawersAreNull) return null;
                Drawer drawer = base.DrawerCreator(parent);
                drawer.OnMouseEnter += OnDrawerMouseEnter;
                drawer.OnMouseLeave += OnDrawerMouseLeave;
                return drawer;
            }

            public bool CanTake()
            {
                if (BattleArea.IsAnyAiming) return false;
                bool result = Side.isMe && Side.CanAfford(this) && _menu._territory.IsPlayerPhase() && !_endTurnDropBlockIsActive;
                if (!result)
                    Drawer.transform.DOAShake();
                return result;
            }
            public bool CanReturn()
            {
                return true;
            }
            public bool CanDropOn(TableField field)
            {
                return Data.IsUsable(new TableFloatCardUseArgs(this, Territory));
            }

            public bool CanPullOut()
            {
                return Side.isMe && Side.CanAfford(this);
            }
            public bool CanPullIn()
            {
                return true;
            }

            public void OnTake()
            {
                AsInSleeve.OnTakeBase();
                _menu._territory.SetCardsColliders(false);
            }
            public void OnReturn()
            {
                AsInSleeve.OnReturnBase();
                _menu._territory.SetFieldsColliders(true);
            }
            public void OnDropOn(TableSleeveCardDropArgs e)
            {
                AsInSleeve.OnDropOnBase(e);
                if (Side.isMe)
                {
                    _menu._territory.SetFieldsColliders(true);
                    Drawer.IsSelected = false;
                }
                if (Drawer.IsFlipped)
                    Drawer.FlipY();

                if (!e.isPreview)
                    _endTurnDropBlockIsActive = _menu._territoryEndTurnIsPending;
                else
                {
                    _endTurnDropBlockIsActive = false;
                    return;
                }

                Side.Purchase(this);
                Drawer.ChangePointer = false;
                _ = Territory.PlaceFloatCard(this, Side);
            }
            public Tween OnPullOut(bool sleevePull)
            {
                return AsInSleeve.OnPullOutBase(sleevePull);
            }
            public Tween OnPullIn(bool sleevePull)
            {
                return AsInSleeve.OnPullInBase(sleevePull);
            }

            void OnDrawerMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                BattleFloatCardDrawer drawer = (BattleFloatCardDrawer)sender;
                if (Sleeve.Contains(this) && !Side.CanAfford(this))
                    drawer.priceIcon.RedrawColor(Color.red);
            }
            void OnDrawerMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                BattleFloatCardDrawer drawer = (BattleFloatCardDrawer)sender;
                drawer.priceIcon.RedrawColor();
            }
        }
        protected class pSleeve : BattleSleeve
        {
            readonly BattlePlaceMenu _menu;

            public pSleeve(BattlePlaceMenu menu, BattleSide side, Transform parent) : base(side, parent) 
            {
                _menu = menu;
                TryOnInstantiatedAction(GetType(), typeof(pSleeve));
            }
            protected pSleeve(pSleeve src, BattleSleeveCloneArgs args) : base(src, args) 
            {
                TryOnInstantiatedAction(GetType(), typeof(pSleeve));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleSleeveCloneArgs cArgs)
                     return new pSleeve(this, cArgs);
                else return null;
            }

            protected override ITableSleeveCard HoldingCardCreator(Card data)
            {
                IBattleSleeveCard card;
                if (data.isField)
                     card = new pFieldCard(_menu, (FieldCard)data, Side);
                else card = new pFloatCard(_menu, (FloatCard)data, Side);
                if (!card.Side.isMe)
                     card.Drawer?.FlipY();
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
        protected class pSide : BattleSide
        {
            readonly BattlePlaceMenu _menu;
            public pSide(BattlePlaceMenu menu, BattleTerritory territory, bool isMe) : base(territory, isMe)
            {
                _menu = menu;
                if (PlayerConfig.psychoMode)
                    Health.OnPreSet.Add(null, PsychoModeOnHealthPreSet);
                TryOnInstantiatedAction(GetType(), typeof(pSide));
            }
            protected pSide(pSide src, BattleSideCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pSide));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleSideCloneArgs cArgs)
                    return new pSide(this, cArgs);
                else return null;
            }

            protected override CardDeck DeckCreator()
            {
                if (isMe)
                     return Player.Deck;
                else return Traveler.NewDeck(default, isMe ? _demoLocStageForPlayer : _demoLocStageForEnemy);
            }
            protected override BattleSleeve SleeveCreator(Transform parent)
            {
                return new pSleeve(_menu, this, parent);
            }

            protected override int HealthAtStartFunc()
            {
                if (isMe)
                     return PlayerConfig.psychoMode ? 1 : _demoLocStageForPlayer * 2;
                else return (_demoLocStageForEnemy * 2 * 1 + ((_demoDifficultyScale - 1) * 2)).Ceiling();
            }
            protected override int GoldAtStartFunc()
            {
                return 1;
            }
            protected override int EtherAtStartFunc()
            {
                return 1;
            }

            protected override async UniTask OnStatPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
            {
                await base.OnStatPostSetBase_TOP(sender, e);
                TableStat stat = (TableStat)sender;
                BattleSide side = (BattleSide)stat.Owner;
                if (!side.isMe || stat.Id != "health" || side.Drawer == null) return;
                float sideHealthRatio = (float)e.newStatValue / side.HealthAtStart;
                _menu.SetDeathsDoorEffect(sideHealthRatio);
            }
            UniTask PsychoModeOnHealthPreSet(object sender, TableStat.PreSetArgs e)
            {
                if (e.deltaValue > 0)
                    e.handled = true;
                return UniTask.CompletedTask;
            }
        }
        protected class pTerritory : BattleTerritory
        {
            public bool BattleStarted => PhasesPassed != 0;
            readonly BattlePlaceMenu _menu;

            public pTerritory(BattlePlaceMenu menu, bool playerMovesFirst, int sizeX) : base(playerMovesFirst, sizeX, menu.Transform) 
            {
                _menu = menu;
                OnPlayerWon.Add(null, _menu.OnPlayerWon);
                OnPlayerLost.Add(null, _menu.OnPlayerLost);
                TryOnInstantiatedAction(GetType(), typeof(pTerritory));
            }
            protected pTerritory(pTerritory src, BattleTerritoryCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pTerritory));
            }
            public override object Clone(CloneArgs args)
            {
                if (args is BattleTerritoryCloneArgs cArgs)
                     return new pTerritory(this, cArgs);
                else return null;
            }

            protected override BattleFieldCard FieldCardCreator(FieldCard data, BattleSide side)
            {
                return new pFieldCard(_menu, data, side);
            }
            protected override BattleFloatCard FloatCardCreator(FloatCard data, BattleSide side)
            {
                return new pFloatCard(_menu, data, side);
            }

            protected override BattleSide PlayerSideCreator()
            {
                return new pSide(_menu, territory: this, isMe: true);
            }
            protected override BattleSide EnemySideCreator()
            {
                return new pSide(_menu, territory: this, isMe: false);
            }

            // NOTE: 'terr.DrawersAreNull' check is required to determine either this territory is a clone for AI (without drawers) or not
            protected override async UniTask OnStartPhaseBase_TOP(object sender, EventArgs e)
            {
                await base.OnStartPhaseBase_TOP(sender, e);
                pTerritory terr = (pTerritory)sender;
                if (!terr.DrawersAreNull)
                    await VFX.CreateText($"ХОД {terr.Turn}", Color.white, Vector3.zero).DOATextPopUp(0.5f).AsyncWaitForCompletion();
            }
            protected override async UniTask OnEnemyPhaseBase_TOP(object sender, EventArgs e)
            {
                await base.OnEnemyPhaseBase_TOP(sender, e);
                pTerritory terr = (pTerritory)sender;
                await SidePlacePhase(terr, terr.Enemy);
            }
            protected override async UniTask OnPlayerPhaseBase_TOP(object sender, EventArgs e)
            {
                await base.OnPlayerPhaseBase_TOP(sender, e);
                pTerritory terr = (pTerritory)sender;
                await SidePlacePhase(terr, terr.Player);
            }

            async UniTask SidePlacePhase(pTerritory sender, BattleSide side)
            {
                // 1. avoids recursion (from side.ai.TryMakeTurn(), it calls PhaseBase events)
                // 2. does not adjust gold and take cards as this method can be called recursively (and virtually) only from other side
                //    (if this side moves first) to save some memory and CPU calculations
                if (sender.DrawersAreNull) return;

                await side.Gold.AdjustValue(1, _menu);
                await side.Sleeve.TakeMissingCards(!(side.Drawer?.SleeveIsVisible ?? false));
                if (side.isMe && !side.ai.IsEnabled)
                    _menu.SetPlayerControls(true);
            }
        }
        protected class pButtonDrawer : Drawer 
        {
            public pButtonDrawer(BattlePlaceMenu menu, string buttonName) : base(null, menu.Transform.Find<SpriteRenderer>(buttonName)) { }
            protected override bool CanBeSelected()
            {
                return base.CanBeSelected() && !ITableSleeveCard.IsHoldingAnyCard;
            }
        }

        public BattlePlaceMenu() : base(ID, _prefab)
        {
            _turnButton = new pButtonDrawer(this, "Turn button");
            _fleeButton = new pButtonDrawer(this, "Flee button");

            _logTextMesh = Transform.Find<TextMeshPro>("Log window/Text");
            _descTextMesh = Transform.Find<TextMeshPro>("Desc window/Text");
            _bg = Transform.Find<SquaresBackground>("BG");
            _bg.manualLightUp = true;

            _turnButton.OnMouseClick += (s, e) => TryEndTurn();
            _turnButton.SetTooltip(() => $"Завершить ход {_territory.Turn}.");

            _fleeButton.OnMouseClick += (s, e) => TryFlee();
            _fleeButton.SetTooltip(() => "Шанс побега: 100%.");

            _logTextMesh.text = Log;
            _descTextMesh.text = "";


            #if !DEMO
            _territory = new pTerritory(this, playerMovesFirst: true /*UnityEngine.Random.value > 0.5f*/, sizeX: 5);
            _territory.player = _territory.player;
            _territory.enemy = _territory.enemy;
            #else
            _demoPrevFieldsCount = 4;
            _demoPrevFloatsCount = 0;
            #endif

            SetBellState(false);
            SetFleeState(false);
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

        public void SetPlayerControls(bool value)
        {
            if (_territory == null) return;
            if (_territory.DrawersAreNull) return;

            SetBellState(value);
            SetFleeState(value);

            _territory.Player.Sleeve.Drawer.PullIn();
            _territory.Enemy.Sleeve.Drawer.PullIn();
        }
        public void Skip(bool force)
        {
            // TODO: remove this function
            if (_territory == null) return;
            if (_territory.DrawersAreNull) return;

            if (force)
                _ = OnPlayerWon(_territory, EventArgs.Empty);
            else
            {
                BattleSide side = _territory.Enemy;
                side.Health.OnPreSet.Add(null, async (s, e) => e.deltaValue = -side.HealthAtStart);
                _ = side.Health.AdjustValue(1, null);
            }
        }

        void TryFlee()
        {
            _territoryEndTurnIsPending = true;
            SetPlayerControls(false);
            PlayerQueue.Enqueue(new PlayerAction()
            {
                conditionFunc = TerritoryPlayerActionsCondition,
                successFunc = () => _ = _territory.TryConclude(),
                abortFunc = TerritoryPlayerActionsAbort,
            });
        }
        void TryEndTurn()
        {
            _territoryEndTurnIsPending = true;
            SetPlayerControls(false);
            PlayerQueue.Enqueue(new PlayerAction()
            {
                conditionFunc = TerritoryPlayerActionsCondition,
                successFunc = () => _ = _territory.NextPhase(),
                abortFunc = TerritoryPlayerActionsAbort,
            });
        }

        protected override void Open()
        {
            base.Open();
            #if DEMO
            demo_OnBattleStart(_demoDifficulty == 0);
            _beatTween = DOVirtual.DelayedCall(1f, demo_BeatTweenUpdate).SetLoops(-1, LoopType.Restart);
            #else
            _territory.NextPhase();
            #endif
        }
        protected override void Close()
        {
            base.Close();
            _beatTween.Kill();
        }
        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            SetPlayerControls(value && (_territory?.BattleStarted ?? false));
            _territory?.SetFieldsColliders(value);
        }

        bool TerritoryPlayerActionsCondition()
        {
            return !_territory.IsConcluded;
        }
        void TerritoryPlayerActionsAbort()
        {
            SetPlayerControls(true);
        }

        async UniTask OnPlayerWon(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (terr.DrawersAreNull) return;

            SetCollider(false);
            #if DEMO
            demo_OnBattleEnd();
            #else
            await TransitFromThis();
            #endif
        }
        async UniTask OnPlayerLost(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (terr.DrawersAreNull) return;

            SetCollider(false);
            #if DEMO
            demo_OnBattleEnd();
            #else
            
            #endif
        }

        void SetDeathsDoorEffect(float sideHealthRatio)
        {
            float ratio = 1 - Mathf.Pow(1 - sideHealthRatio.Clamped(0, 1), 2);
            float duration = ratio == 0 ? 2.0f : 0.5f;

            float pitchFrom = SFX.MusicPitchScale;
            float pitchTo = ratio == 0 ? 0 : 1f - 0.1f * (1f - ratio);

            float volumeFrom = SFX.MusicVolumeScale;
            float volumeTo = 1f - 0.20f * (1f - ratio);

            Global.Volume.profile.TryGet(out ColorAdjustments component);
            component.saturation.overrideState = true;
            component.saturation.value = -100 * (1f - ratio);

            _deathsDoorPitchTween = DOVirtual.Float(pitchFrom, pitchTo, duration, v => SFX.MusicPitchScale = v).SetEase(Ease.OutCubic);
            _deathsDoorVolumeTween = DOVirtual.Float(volumeFrom, volumeTo, duration, v => SFX.MusicVolumeScale = v).SetEase(Ease.OutCubic);
        }
        void ResetDeathsDoorEffect()
        {
            const float DURATION = 2.0f;

            Global.Volume.profile.TryGet(out ColorAdjustments component);
            component.saturation.value = 0;

            _deathsDoorPitchTween.Kill();
            _deathsDoorVolumeTween.Kill();

            _deathsDoorPitchTween = DOVirtual.Float(SFX.MusicPitchScale, 1f, DURATION, v => SFX.MusicPitchScale = v).SetEase(Ease.OutCubic);
            _deathsDoorVolumeTween = DOVirtual.Float(SFX.MusicVolumeScale, 1f, DURATION, v => SFX.MusicVolumeScale = v).SetEase(Ease.OutCubic);
        }
        void ResetDeathsDoorEffectInstantly()
        {
            Global.Volume.profile.TryGet(out ColorAdjustments component);
            component.saturation.value = 0;

            _deathsDoorPitchTween.Kill();
            SFX.MusicPitchScale = 1f;

            _deathsDoorVolumeTween.Kill();
            SFX.MusicVolumeScale = 1f;
        }

        void SetBellState(bool value)
        {
            _turnButton.ColliderEnabled = value;
            _turnButton.gameObject.GetComponent<SpriteRenderer>().color = value ? Color.white : Color.white.WithAlpha(0.5f);
        }
        void SetFleeState(bool value)
        {
            value = false; // TODO: remove
            _fleeButton.ColliderEnabled = value;
            _fleeButton.gameObject.GetComponent<SpriteRenderer>().color = value ? Color.white : Color.white.WithAlpha(0.5f);
        }

        #if DEMO
        Menu demo_CreateCardChooseAndUpgrade(int cPointsPerCard, int cFieldChoices, int cFloatChoices, int cCardsPerChoice, int uPointsLimit)
        {
            if (cFieldChoices == 0 && cFloatChoices == 0)
                return demo_CreateCardUpgrade(uPointsLimit);
            CardChooseMenu menu = new(cPointsPerCard, cFieldChoices, cFloatChoices, cCardsPerChoice, PlayerConfig.rerollsAtStart - _demoDifficulty);
            menu.MenuWhenClosed = () => demo_CreateCardUpgrade(uPointsLimit);
            menu.OnClosed += menu.TryDestroy;
            return menu;
        }
        Menu demo_CreateCardUpgrade(int uPointsLimit)
        {
            CardUpgradeMenu menu = new(Player.Deck, uPointsLimit);
            menu.MenuWhenClosed = () => this;
            menu.OnClosed += menu.TryDestroy;
            return menu;
        }

        async void demo_OnBattleEnd()
        {
            Global.OnUpdate -= demo_SpeedUpUpdate;
            DOTween.timeScale = 1f;

            if (Territory.Player.Health <= 0)
            {
                Traveler.TryStopTravel(complete: false);
                await VFX.CreateScreenBG(Color.clear, Transform).DOFade(1, 0.5f).AsyncWaitForCompletion();
                TextMeshPro text = VFX.CreateText($"КОНЕЦ\n<size=50%>время: {demo_TimeSpanFormat()}\n\n[Enter] главное меню\n[Shift+Esc] сдаться", Color.white, Transform);
                text.transform.DOAShake();
                text.sortingOrder = 600;
                Global.OnUpdate += demo_EndChoiceOnUpdate;
                SFX.StopMusic();
            }
            else if (_demoDifficulty == DEMO_DIFFICULTY_MID)
            {
                SpriteRenderer bg = VFX.CreateScreenBG(Color.clear, Transform);
                bg.name = "Black bg";
                await bg.DOFade(1, 0.5f).AsyncWaitForCompletion();
                TextMeshPro text = VFX.CreateText($"ФИНАЛ?\n<size=50%>время: {demo_TimeSpanFormat()}\n\n<size=50%>[Enter] начать хардкор\n[Shift+Esc] сбежать", Color.white, Transform);
                text.sortingOrder = 410;
                text.transform.DOAShake();
                text.name = "Black text";
                Global.OnUpdate += demo_StageFiveChoiceOnUpdate;
            }
            else if (_demoDifficulty == DEMO_DIFFICULTY_MAX)
            {
                SpriteRenderer bg = VFX.CreateScreenBG(Color.clear, Transform);
                bg.name = "Black bg";
                await bg.DOFade(1, 0.5f).AsyncWaitForCompletion();
                string headerStr = PlayerConfig.psychoMode ? "ФИНАЛ" : "ФИНАЛ НА ПСИХЕ";
                TextMeshPro text = VFX.CreateText($"{headerStr}\n<size=50%>время: {demo_TimeSpanFormat()}\n\n[Enter] главное меню\n[Shift+Esc] уйти красиво", Color.white, Transform);
                text.sortingOrder = 410;
                text.transform.DOAShake();
                text.name = "Black text";
                Global.OnUpdate += demo_EndChoiceOnUpdate;
            }
            else demo_OnBattleEndBase();
        }
        async void demo_OnBattleEndBase()
        {
            //int fieldCards = Player.Deck.fieldCards.Count;
            //int floatCards = Player.Deck.floatCards.Count;

            int pointsPerCard = demo_DifficultyToLocStage(_demoDifficulty + 1);
            Traveler.DeckCardsCount(pointsPerCard, out int fieldCardsRecommended, out int floatCardsRecommended);
            int pointsLimit = fieldCardsRecommended * pointsPerCard;

            int fieldsChoices = (fieldCardsRecommended - _demoPrevFieldsCount).ClampedMin(1);
            int floatsChoices = _demoDifficulty >= 3 && _demoDifficulty <= DEMO_DIFFICULTY_MID ? 1 : 0;
            int cardsPerChoice = (_demoDifficulty + 2).ClampedMax(5);

            _demoPrevFieldsCount = fieldCardsRecommended;
            _demoPrevFloatsCount = floatCardsRecommended;

            Menu menu = demo_CreateCardChooseAndUpgrade(pointsPerCard, fieldsChoices, floatsChoices, cardsPerChoice, pointsLimit);
            await MenuTransit.Between(this, menu, ResetDeathsDoorEffect);
        }
        async void demo_OnBattleStart(bool firstOpening)
        {
            for (int i = 0; i < _demoPalettes[_demoDifficulty].Length; i++)
                ColorPalette.All[i].ColorCur = _demoPalettes[_demoDifficulty][i];

            _demoDifficulty++;

            int locStage = demo_DifficultyToLocStage(_demoDifficulty);
            _demoDifficultyScale = demo_DifficultyToLocStageScale(_demoDifficulty);
            _demoLocStageForPlayer = locStage;
            _demoLocStageForEnemy = (locStage * _demoDifficultyScale).Ceiling();

            _territory?.Dispose();
            _territory = new pTerritory(this, demo_DifficultyToPlayerIfFirst(_demoDifficulty), sizeX: 5);
            _territory.Enemy.ai.Style = (BattleAI.PlayStyle)UnityEngine.Random.Range(0, 3);
            //_territory.Enemy.Drawer.SleeveIsVisible = true; // TODO: remove

            SpriteRenderer bg = VFX.CreateScreenBG(firstOpening ? Color.black : Color.clear);
            await UniTask.Delay(500);
            string text1 = $"ЭТАП {_demoDifficulty}/{DEMO_DIFFICULTY_MID}";
            string text2Hex = ColorPalette.C2.Hex;
            string text2 = $"\n<size=50%><color={text2Hex}>угроза: {_demoLocStageForPlayer}    сложность: {_demoDifficultyScale * 100}%";
            string text3 = _territory.PlayerMovesFirst ? $"\nвы ходите первым" : "\nвы ходите последним";

            if (_demoDifficulty >= DEMO_DIFFICULTY_MID)
                _fleeButton.SetTooltip("<color=red>ТЕБЕ НЕ СБЕЖАТЬ.");

            TextMeshPro bgText = VFX.CreateText(text1, Color.white, bg.transform);
            bgText.transform.position = Vector2.up * 136;
            bgText.transform.DOAShake();
            bgText.richText = true;
            bgText.sortingOrder = bg.sortingOrder + 1;
            bgText.alignment = TextAlignmentOptions.Top;

            await UniTask.Delay(500);
            bgText.transform.DOAShake();
            bgText.text += text2;

            await UniTask.Delay(500);
            bgText.transform.DOAShake();
            bgText.text += text3;

            await UniTask.Delay(2000);
            bg.DOFade(0, 0.5f);
            await bgText.DOFade(0, 0.5f).SetDelay(1f).AsyncWaitForCompletion();

            Global.OnUpdate += demo_SpeedUpUpdate;
            bg.gameObject.Destroy();
            await _territory.NextPhase();
        }

        static bool demo_DifficultyToPlayerIfFirst(int difficulty)
        {
            return difficulty % 2 != 0;
        }
        static float demo_DifficultyToLocStageScale(int difficulty) => difficulty switch
        {
            1 => 0.80f,
            2 => 1.00f,
            3 => 1.20f,
            4 => 1.40f,
            5 => 1.60f,
            6 => 1.80f,
            7 => 2.00f,
            _ => throw new NotSupportedException(),
        };
        static int demo_DifficultyToLocStage(int difficulty) => difficulty switch
        {
            1 => 8,
            2 => 16,
            3 => 24,
            4 => 32,
            5 => 40,
            6 => 48,
            7 => 56,
            _ => throw new NotSupportedException(),
        };
        static string demo_TimeSpanFormat()
        {
            TimeSpan span = DateTime.Now - Global.Menu.GameStartTime;
            int hours = span.Hours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;
            string hoursStr = hours < 10 ? "0" + hours : hours.ToString();
            string minutesStr = minutes < 10 ? "0" + minutes : minutes.ToString();
            string secondsStr = seconds < 10 ? "0" + seconds : seconds.ToString();
            return $"{hoursStr}:{minutesStr}:{secondsStr}";
        }

        void demo_EndChoiceOnUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.Return))
                return;

            Global.OnUpdate -= demo_EndChoiceOnUpdate;
            _demoDifficulty = 0;
            SFX.ResetMusicMixes();
            ResetDeathsDoorEffectInstantly();
            SFX.MusicVolumeScale = 0f;
            OnClosed += TryDestroy;
            MenuTransit.Between(this, Global.Menu, () => SFX.MusicVolumeScale = 2f);
        }
        void demo_StageFiveChoiceOnUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.Return))
                return;

            Global.OnUpdate -= demo_StageFiveChoiceOnUpdate;
            demo_OnBattleEndBase();
            DOVirtual.DelayedCall(1, () =>
            {
                Transform.Find("Black text").gameObject.Destroy();
                Transform.Find("Black bg").gameObject.Destroy();
            });
        }
        void demo_SpeedUpUpdate()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                DOTween.timeScale = 3f;
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                DOTween.timeScale = 1f;
        }
        void demo_BeatTweenUpdate()
        {
            if (!IsOpened) return;
            _bg.LightUpSquares(1);
        }
        #endif
    }
}
