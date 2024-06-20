using Cysharp.Threading.Tasks;
using Game.Cards;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровую территорию на столе из массива игровых полей, где её половина принадлежит одной из сторон сражения и где карты могут инициировать действия.
    /// </summary>
    public class BattleTerritory : TableTerritory
    {
        public const int PLAYER_FIELDS_Y = 0;
        public const int ENEMY_FIELDS_Y = 1;

        protected const int START_PHASE_INDEX = 0;
        protected const int END_PHASE_INDEX = 3;

        public IIdEventVoidAsync<BattleField> OnCardAttachedToField => _onCardAttachedToField;
        public IIdEventVoidAsync<BattleField> OnCardDetatchedFromField => _onCardDetatchedFromField;

        public IIdEventVoidAsync OnStartPhase => _onStartPhase;
        public IIdEventVoidAsync OnEnemyPhase => _onEnemyPhase;
        public IIdEventVoidAsync OnPlayerPhase => _onPlayerPhase;
        public IIdEventVoidAsync OnEndPhase => _onEndPhase;
        public IIdEventVoidAsync OnNextPhase => _onNextPhase;
        public IIdEventVoidAsync OnPlayerWon => _onPlayerWon;
        public IIdEventVoidAsync OnPlayerLost => _onPlayerLost;

        public bool PhaseCycleEnabled
        {
            get => _phaseCycleEnabled;
            set => _phaseCycleEnabled = value;
        }
        public bool PhaseAwaitsPlayer => _currentPhaseCycleIndex == _playerPhaseCycleIndex;
        public BattleInitiationQueue Initiations => _initiations;

        public BattleSide PhaseSide => PhaseAwaitsPlayer ? player : enemy;
        public BattleSide WaitingSide => PhaseAwaitsPlayer ? enemy : player;

        public int PhaseCyclesPassed => _phaseCyclesPassed;
        public int PhasesPassed => _phasesPassed;
        public int Turn => _phaseCyclesPassed + 1;
        protected int CurrentPhaseCycleIndex => _currentPhaseCycleIndex;

        public readonly BattleSide player;
        public readonly BattleSide enemy;

        readonly TableEventVoid<BattleField> _onCardAttachedToField;
        readonly TableEventVoid<BattleField> _onCardDetatchedFromField;

        readonly TableEventVoid _onStartPhase;
        readonly TableEventVoid _onEnemyPhase;
        readonly TableEventVoid _onPlayerPhase;
        readonly TableEventVoid _onEndPhase;
        readonly TableEventVoid _onNextPhase;
        readonly TableEventVoid _onPlayerWon;
        readonly TableEventVoid _onPlayerLost;

        readonly BattleField[,] _fields;
        readonly BattleInitiationQueue _initiations;
        readonly List<TableEventVoid> _phaseCycleEvents;
        readonly bool _playerMovesFirst;
        readonly int _playerPhaseCycleIndex;

        bool _phaseCycleEnabled;
        int _phaseCyclesPassed;
        int _phasesPassed;
        int _currentPhaseCycleIndex;

        public BattleTerritory(bool playerMovesFirst, int sizeX, Transform parent, bool createFields = true, bool withDrawer = true) 
            : base(new int2(sizeX, 2), parent, createFields: false, withDrawer)
        {
            _onCardAttachedToField = new TableEventVoid<BattleField>();
            _onCardDetatchedFromField = new TableEventVoid<BattleField>();

            _onStartPhase = new TableEventVoid();
            _onEnemyPhase = new TableEventVoid();
            _onPlayerPhase = new TableEventVoid();
            _onEndPhase = new TableEventVoid();
            _onNextPhase = new TableEventVoid();
            _onPlayerWon = new TableEventVoid();
            _onPlayerLost = new TableEventVoid();

            _onStartPhase.Add(OnStartPhaseBase);
            _onEnemyPhase.Add(OnEnemyPhaseBase);
            _onPlayerPhase.Add(OnPlayerPhaseBase);
            _onEndPhase.Add(OnEndPhaseBase);
            _onNextPhase.Add(OnNextPhaseBase);
            _onPlayerWon.Add(OnPlayerWonBase);
            _onPlayerLost.Add(OnPlayerLostBase);

            _fields = new BattleField[sizeX, 2];
            _initiations = new BattleInitiationQueue(this);
            _initiations.OnStarted += OnInitiationsProcessingStartBase;
            _initiations.OnEnded += OnInitiationsProcessingEndBase;

            _phaseCycleEvents = new List<TableEventVoid>();
            _playerMovesFirst = playerMovesFirst;
            _playerPhaseCycleIndex = _playerMovesFirst ? 1 : 2;

            _phaseCycleEnabled = true;
            _phaseCyclesPassed = 0;
            _phasesPassed = 0;
            _currentPhaseCycleIndex = -1;

            _phaseCycleEvents.Add(_onStartPhase);
            if (_playerMovesFirst)
            {
                _phaseCycleEvents.Add(_onPlayerPhase);
                _phaseCycleEvents.Add(_onEnemyPhase);
            }
            else
            {
                _phaseCycleEvents.Add(_onEnemyPhase);
                _phaseCycleEvents.Add(_onPlayerPhase);
            }
            _phaseCycleEvents.Add(_onEndPhase);

            player = PlayerSideCreator();
            player.health.OnPostSet.Add(OnSideHealthSet);

            enemy = EnemySideCreator();
            enemy.health.OnPostSet.Add(OnSideHealthSet);

            if (player.isMe == enemy.isMe)
                throw new Exception($"BattleTerritory should contain only one side with {nameof(BattleSide.isMe)} set to 'true'.");

            if (createFields) CreateFields();
        }
        protected BattleTerritory(BattleTerritory src, BattleTerritoryCloneArgs args) : base(src, args)
        {
            _onCardAttachedToField = (TableEventVoid<BattleField>)src._onCardAttachedToField.Clone();
            _onCardDetatchedFromField = (TableEventVoid<BattleField>)src._onCardDetatchedFromField.Clone();

            _onStartPhase = (TableEventVoid)src._onStartPhase.Clone();
            _onEnemyPhase = (TableEventVoid)src._onEnemyPhase.Clone();
            _onPlayerPhase = (TableEventVoid)src._onPlayerPhase.Clone();
            _onEndPhase = (TableEventVoid)src._onEndPhase.Clone();
            _onNextPhase = (TableEventVoid)src._onNextPhase.Clone();
            _onPlayerWon = (TableEventVoid)src._onPlayerWon.Clone();
            _onPlayerLost = (TableEventVoid)src._onPlayerLost.Clone();

            _fields = new BattleField[src.grid.x, src.grid.y];
            _initiations = new BattleInitiationQueue(this);
            _initiations.OnStarted += OnInitiationsProcessingStartBase;
            _initiations.OnEnded += OnInitiationsProcessingEndBase;

            _phaseCycleEvents = new List<TableEventVoid>(src._phaseCycleEvents);
            _playerMovesFirst = src._playerMovesFirst;
            _playerPhaseCycleIndex = src._playerPhaseCycleIndex;

            _phaseCycleEnabled = src._phaseCycleEnabled;
            _phaseCyclesPassed = src._phaseCyclesPassed;
            _phasesPassed = src._phasesPassed;
            _currentPhaseCycleIndex = src._currentPhaseCycleIndex;

            BattleSide playerClone = PlayerSideCloner(src.player, args);
            BattleSide enemyClone = EnemySideCloner(src.enemy, args);

            player = playerClone;
            enemy = enemyClone;

            args.TryOnClonedAction(src.GetType(), typeof(BattleTerritory));
        }

        public override void Dispose()
        {
            base.Dispose();

            OnStartPhase.Clear();
            OnEnemyPhase.Clear();
            OnPlayerPhase.Clear();
            OnEndPhase.Clear();
            OnNextPhase.Clear();

            _initiations.Dispose();
            _phaseCycleEvents.Clear();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleTerritoryCloneArgs cArgs)
                return new BattleTerritory(this, cArgs);
            else return null;
        }

        public void NextPhase()
        {
            if (!_phaseCycleEnabled) return;

            _currentPhaseCycleIndex++;
            _phasesPassed++;

            if (_currentPhaseCycleIndex >= _phaseCycleEvents.Count)
            {
                _currentPhaseCycleIndex = 0;
                _phaseCyclesPassed++;
            }

            _onNextPhase.Invoke(this, EventArgs.Empty);
            _phaseCycleEvents[_currentPhaseCycleIndex].Invoke(this, EventArgs.Empty);
        }
        public void LastPhase()
        {
            int repeats = END_PHASE_INDEX - _currentPhaseCycleIndex;
            for (int i = 0; i < repeats; i++)
                NextPhase();
        }
        public BattleSide GetOppositeOf(BattleSide side)
        {
            return player == side ? enemy : player;
        }

        public UniTask PlaceFieldCard(FieldCard data, BattleField field,  ITableEntrySource by)
        {
            BattleFieldCard card = FieldCardCreator(data, field.Side);
            return PlaceFieldCard(card, field, by);
        }
        public UniTask PlaceFloatCard(FloatCard data, BattleSide side, ITableEntrySource by)
        {
            BattleFloatCard card = FloatCardCreator(data, side);
            return PlaceFloatCard(card, by);
        }

        // use to write logs and (set field/use card) automatically 
        public async UniTask PlaceFieldCard(BattleFieldCard card, BattleField field, ITableEntrySource by)
        {
            if (by != null)
                 WriteLog($"{by.TableName} устанавливает карту {card.Data.name} на поле {field.PosToStringRich()}.");
            else WriteLog($"Установка карты {card.Data.name} на поле {field.PosToStringRich()}.");
            await card.AttachToAnotherField(field);
        }
        public async UniTask PlaceFloatCard(BattleFloatCard card, ITableEntrySource by)
        {
            if (!card.TryUse())
            {
                Debug.LogError("Float card cannot be placed (used) on territory.");
                return;
            }

            WriteLog($"{by.TableName} использует карту {card.Data.name}.");
            await card.AwaitUse();
        }

        public new BattleField Field(in int2 pos)
        {
            return Field(pos.x, pos.y);
        }
        public new BattleField Field(in int x, in int y)
        {
            return _fields[x, y];
        }

        public new BattleField FieldOpposite(in int2 pos)
        {
            return FieldOpposite(pos.x, pos.y);
        }
        public new BattleField FieldOpposite(in int x, in int y)
        {
            return _fields[x, y == 0 ? 1 : 0];
        }

        public new IEnumerable<BattleField> Fields()
        {
            foreach (BattleField field in _fields)
            {
                if (field != null)
                    yield return field;
            }
        }
        public new IEnumerable<BattleField> Fields(int2 centerPos, TerritoryRange range)
        {
            if (range == TerritoryRange.none)
                yield break;
            if (range == TerritoryRange.all)
            {
                foreach (BattleField field in _fields)
                    yield return field;

                yield break;
            }
            if (range == TerritoryRange.ownerSingle)
            {
                yield return _fields[centerPos.x, centerPos.y];
                yield break;
            }

            bool FilterBase(int2 pos) => pos.x < grid.x && pos.y < grid.y && _fields[pos.x, pos.y] != null;
            Predicate<int2> filter;

            bool exludeSelf = range.targets.HasFlag(TerritoryTargets.NotSelf);
            if (exludeSelf)
                filter = pos => FilterBase(pos) && !pos.Equals(centerPos);
            else filter = FilterBase;

            foreach (int2 pos in range.Overlap(centerPos, filter))
                yield return _fields[pos.x, pos.y];
        }

        // do NOT set card.Field in this method (see PlaceFieldCard())
        protected virtual BattleFieldCard FieldCardCreator(FieldCard data, BattleSide side)
        {
            return new BattleFieldCard(data, side);
        }
        protected virtual BattleFloatCard FloatCardCreator(FloatCard data, BattleSide side)
        {
            return new BattleFloatCard(data, side);
        }

        protected virtual BattleSide PlayerSideCreator()
        {
            return new BattleSide(this, true);
        }
        protected virtual BattleSide PlayerSideCloner(BattleSide src, BattleTerritoryCloneArgs args)
        {
            BattleSideCloneArgs sideCArgs = new(this, args);
            return (BattleSide)src.Clone(sideCArgs);
        }

        protected virtual BattleSide EnemySideCreator()
        {
            return new BattleSide(this, false);
        }
        protected virtual BattleSide EnemySideCloner(BattleSide src, BattleTerritoryCloneArgs args)
        {
            return PlayerSideCloner(src, args);
        }

        protected override TableField FieldCreator(int x, int y)
        {
            BattleField field = new(y == 0 ? player : enemy, new int2(x, y), transformForFields, HasFieldDrawers);
            field.OnCardAttached.Add(OnCardAttachedToFieldBase);
            field.OnCardDetatched.Add(OnCardDetatchedFromFieldBase);
            return field;
        }
        protected override TableField FieldCloner(TableField src, TableTerritoryCloneArgs args)
        {
            BattleField srcCast = (BattleField)src;
            BattleTerritoryCloneArgs argsCast = (BattleTerritoryCloneArgs)args;

            FieldCard srcFieldCardDataClone = (FieldCard)srcCast.Card?.Data.Clone();
            BattleSide side = src.pos.y == PLAYER_FIELDS_Y ? player : enemy;
            BattleFieldCloneArgs fieldCArgs = new(srcFieldCardDataClone, side, argsCast);
            return (BattleField)src.Clone(fieldCArgs);
        }

        protected override bool FieldDestroyFilter(int x, int y)
        {
            return true;
        }
        protected override void FieldSetter(int x, int y, TableField value)
        {
            base.FieldSetter(x, y, value);
            _fields[x, y] = (BattleField)value;
        }

        protected virtual UniTask OnStartPhaseBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            terr.WriteLog($"-- ХОД {terr.Turn} (установка) --");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEnemyPhaseBase(object sender, EventArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerPhaseBase(object sender, EventArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEndPhaseBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            terr.WriteLog($"-- ХОД {terr.Turn} (атака) --");
            foreach (BattleField field in terr.Fields().WithCard())
                terr.Initiations.Enqueue(field.Card.CreateInitiation());
            terr.Initiations.Run();
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnNextPhaseBase(object sender, EventArgs e)
        {
            BattleArea.StopTargetAiming();
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerWonBase(object sender, EventArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerLostBase(object sender, EventArgs e)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnInitiationsProcessingStartBase(object sender, EventArgs e) { }
        protected virtual void OnInitiationsProcessingEndBase(object sender, EventArgs e)
        {
            BattleInitiationQueue queue = (BattleInitiationQueue)sender;
            BattleTerritory terr = queue.Territory;
            if (terr._currentPhaseCycleIndex == END_PHASE_INDEX)
                terr.NextPhase();
        }

        async UniTask OnSideHealthSet(object sender, TableStat.PostSetArgs e)
        {
            if (e.newStatValue > 0) return;
            _phaseCycleEnabled = false;
            _initiations.OnceComplete += (s, e) =>
            {
                if (player.health > 0 && enemy.health > 0)
                {
                    _phaseCycleEnabled = true;
                    NextPhase();
                    return;
                }

                SetColliders(false);
                if (player.health > 0)
                     _onPlayerWon.Invoke(this, EventArgs.Empty);
                else _onPlayerLost.Invoke(this, EventArgs.Empty);
            };
        }
        async UniTask OnCardAttachedToFieldBase(object sender, EventArgs e)
        {
            BattleField field = (BattleField)sender;
            BattleTerritory terr = field.Territory;
            await terr._onCardAttachedToField.Invoke(terr, field);
        }
        async UniTask OnCardDetatchedFromFieldBase(object sender, EventArgs e)
        {
            BattleField field = (BattleField)sender;
            BattleTerritory terr = field.Territory;
            await terr._onCardDetatchedFromField.Invoke(terr, field);
        }
    }
}
