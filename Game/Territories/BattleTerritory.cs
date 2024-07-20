using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Menus;
using System;
using System.Collections.Generic;
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

        public IIdEventVoidAsync OnStartPhase => _onStartPhase;
        public IIdEventVoidAsync OnEnemyPhase => _onEnemyPhase;
        public IIdEventVoidAsync OnPlayerPhase => _onPlayerPhase;
        public IIdEventVoidAsync OnEndPhase => _onEndPhase;
        public IIdEventVoidAsync OnNextPhase => _onNextPhase;
        public IIdEventVoidAsync OnPlayerWon => _onPlayerWon;
        public IIdEventVoidAsync OnPlayerLost => _onPlayerLost;

        public bool Concluded => _concluded;
        public bool PlayerMovesFirst => _playerMovesFirst;
        public bool PhaseCycleEnabled
        {
            get => _phaseCycleEnabled;
            set => _phaseCycleEnabled = value;
        }
        public bool PhaseAwaitsPlayer => _currentPhaseCycleIndex == _playerPhaseCycleIndex;
        public BattleInitiationQueue Initiations => _initiations;

        public BattleSide PhaseSide => GetPhaseSide();
        public BattleSide WaitingSide => GetWaitingSide();

        public int PhaseCyclesPassed => _phaseCyclesPassed;
        public int PhasesPassed => _phasesPassed;
        public int Turn => _phaseCyclesPassed + 1;
        protected int CurrentPhaseCycleIndex => _currentPhaseCycleIndex;

        public BattleSide Player => _player;
        public BattleSide Enemy => _enemy;

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
        readonly string _eventsGuid;

        BattleSide _player;
        BattleSide _enemy;
        bool _concluded;
        bool _phaseCycleEnabled;
        int _phaseCyclesPassed;
        int _phasesPassed;
        int _currentPhaseCycleIndex;

        public BattleTerritory(bool playerMovesFirst, int sizeX, Transform parent, bool createFields = true) : base(new int2(sizeX, 2), parent, false)
        {
            _fields = new BattleField[sizeX, 2];
            _initiations = new BattleInitiationQueue(this);
            _initiations.OnStarted += OnInitiationsProcessingStartBase;
            _initiations.OnEnded += OnInitiationsProcessingEndBase;

            _phaseCycleEvents = new List<TableEventVoid>();
            _playerMovesFirst = playerMovesFirst;
            _playerPhaseCycleIndex = _playerMovesFirst ? 1 : 2;
            _eventsGuid = this.GuidGen(2);

            _phaseCycleEnabled = true;
            _phaseCyclesPassed = 0;
            _phasesPassed = 0;
            _currentPhaseCycleIndex = -1;

            _onStartPhase = new TableEventVoid();
            _onEnemyPhase = new TableEventVoid();
            _onPlayerPhase = new TableEventVoid();
            _onEndPhase = new TableEventVoid();
            _onNextPhase = new TableEventVoid();
            _onPlayerWon = new TableEventVoid();
            _onPlayerLost = new TableEventVoid();

            _onStartPhase.Add(_eventsGuid, OnStartPhaseBase);
            _onEnemyPhase.Add(_eventsGuid, OnEnemyPhaseBase);
            _onPlayerPhase.Add(_eventsGuid, OnPlayerPhaseBase);
            _onEndPhase.Add(_eventsGuid, OnEndPhaseBase);
            _onNextPhase.Add(_eventsGuid, OnNextPhaseBase);
            _onPlayerWon.Add(_eventsGuid, OnPlayerWonBase);
            _onPlayerLost.Add(_eventsGuid, OnPlayerLostBase);

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

            AddOnInstantiatedAction(GetType(), typeof(BattleTerritory), () =>
            {
                _player = PlayerSideCreator();
                _player.health.OnPostSet.Add(_eventsGuid, OnSideHealthSet);

                _enemy = EnemySideCreator();
                _enemy.health.OnPostSet.Add(_eventsGuid, OnSideHealthSet);

                if (_player.isMe == _enemy.isMe)
                    throw new Exception($"BattleTerritory should contain only one side with {nameof(BattleSide.isMe)} set to 'true'.");
                if (createFields)
                    CreateFields();
            });
        }
        protected BattleTerritory(BattleTerritory src, BattleTerritoryCloneArgs args) : base(src, args)
        {
            _onStartPhase = (TableEventVoid)src._onStartPhase.Clone();
            _onEnemyPhase = (TableEventVoid)src._onEnemyPhase.Clone();
            _onPlayerPhase = (TableEventVoid)src._onPlayerPhase.Clone();
            _onEndPhase = (TableEventVoid)src._onEndPhase.Clone();
            _onNextPhase = (TableEventVoid)src._onNextPhase.Clone();
            _onPlayerWon = (TableEventVoid)src._onPlayerWon.Clone();
            _onPlayerLost = (TableEventVoid)src._onPlayerLost.Clone();

            _fields = new BattleField[src.Grid.x, src.Grid.y];
            _initiations = new BattleInitiationQueue(this);
            _initiations.OnStarted += OnInitiationsProcessingStartBase;
            _initiations.OnEnded += OnInitiationsProcessingEndBase;

            _phaseCycleEvents = new List<TableEventVoid>();
            _playerMovesFirst = src._playerMovesFirst;
            _playerPhaseCycleIndex = src._playerPhaseCycleIndex;

            _phaseCycleEnabled = src._phaseCycleEnabled;
            _phaseCyclesPassed = src._phaseCyclesPassed;
            _phasesPassed = src._phasesPassed;
            _currentPhaseCycleIndex = src._currentPhaseCycleIndex;

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

            _player = PlayerSideCloner(src._player, args);
            _enemy = EnemySideCloner(src._enemy, args);
            TryOnInstantiatedAction(GetType(), typeof(BattleTerritory));
        }

        public override void Dispose()
        {
            base.Dispose();

            OnStartPhase.Clear();
            OnEnemyPhase.Clear();
            OnPlayerPhase.Clear();
            OnEndPhase.Clear();
            OnNextPhase.Clear();

            _player.Dispose();
            _enemy.Dispose();

            _initiations.Dispose();
            _phaseCycleEvents.Clear();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleTerritoryCloneArgs cArgs)
                return new BattleTerritory(this, cArgs);
            else return null;
        }

        public async UniTask NextPhase()
        {
            if (!_phaseCycleEnabled) return;
            TableEventManager.Add();

            _currentPhaseCycleIndex++;
            _phasesPassed++;

            if (_currentPhaseCycleIndex >= _phaseCycleEvents.Count)
            {
                _currentPhaseCycleIndex = 0;
                _phaseCyclesPassed++;
            }

            await _onNextPhase.Invoke(this, EventArgs.Empty);
            await _phaseCycleEvents[_currentPhaseCycleIndex].Invoke(this, EventArgs.Empty);
            TableEventManager.Remove();
        }
        public async UniTask LastPhase()
        {
            int repeats = END_PHASE_INDEX - _currentPhaseCycleIndex;
            for (int i = 0; i < repeats; i++)
                await NextPhase();
        }
        public async UniTask Conclude(bool playerWon)
        {
            if (_concluded) return;
            _concluded = true;
            if (playerWon)
                 await _onPlayerWon.Invoke(this, EventArgs.Empty);
            else await _onPlayerLost.Invoke(this, EventArgs.Empty);
        }
        public BattleSide GetOppositeOf(BattleSide side)
        {
            return _player == side ? _enemy : _player;
        }
        
        public UniTask<BattleFieldCard> PlaceFieldCard(FieldCard data, BattleField field,  ITableEntrySource source)
        {
            if (data == null) throw new NullReferenceException();
            BattleFieldCard card = FieldCardCreator(data, field.Side);
            return PlaceFieldCard(card, field, source);
        }
        public UniTask<BattleFloatCard> PlaceFloatCard(FloatCard data, BattleSide side, ITableEntrySource source)
        {
            if (data == null) throw new NullReferenceException();
            BattleFloatCard card = FloatCardCreator(data, side);
            return PlaceFloatCard(card, source);
        }

        public async UniTask<BattleFieldCard> PlaceFieldCard(BattleFieldCard card, BattleField field, ITableEntrySource source)
        {
            if (card == null) throw new NullReferenceException();
            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;

            if (card.Drawer != null)
            {
                if (sourceName != null)
                    Menu.WriteLogToCurrent($"Установка карты {card.Data.name} на поле {field.TableName} от {sourceName}.");
                else Menu.WriteLogToCurrent($"Установка карты {card.Data.name} на поле {field.TableName}.");
            }

            TableConsole.LogToFile("terr", $"{TableNameDebug}: field card placement: id: {card.Data.id}, field: {field.TableNameDebug} (by: {sourceNameDebug}).");
            await card.AttachToField(field, source);
            return card;
        }
        public async UniTask<BattleFloatCard> PlaceFloatCard(BattleFloatCard card, ITableEntrySource source)
        {
            if (card == null) throw new NullReferenceException();
            if (!card.TryUse())
                throw new InvalidOperationException("Float card cannot be placed (used) on territory.");

            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;

            if (card.Drawer != null)
            {
                if (sourceName != null)
                    Menu.WriteLogToCurrent($"Использование карты {card.Data.name} от {sourceName}.");
                else Menu.WriteLogToCurrent($"Использование карты {card.Data.name}.");
            }

            TableConsole.LogToFile("terr", $"{TableNameDebug}: float card placement: id: {card.Data.id} (by: {sourceNameDebug}).");
            return card;
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

            bool FilterBase(int2 pos) => pos.x < Grid.x && pos.y < Grid.y && _fields[pos.x, pos.y] != null;
            Predicate<int2> filter;

            bool exludeSelf = range.targets.HasFlag(TerritoryTargets.NotSelf);
            if (exludeSelf)
                filter = pos => FilterBase(pos) && !pos.Equals(centerPos);
            else filter = FilterBase;

            foreach (int2 pos in range.Overlap(centerPos, filter))
                yield return _fields[pos.x, pos.y];
        }

        protected override TableField FieldCreator(int x, int y)
        {
            return new BattleField(y == 0 ? _player : _enemy, new int2(x, y), TransformForFields);
        }
        protected override TableField FieldCloner(TableField src, TableTerritoryCloneArgs args)
        {
            BattleTerritoryCloneArgs argsCast = (BattleTerritoryCloneArgs)args;
            BattleSide side = src.pos.y == PLAYER_FIELDS_Y ? _player : _enemy;
            BattleFieldCloneArgs fieldCArgs = new(side, this, argsCast);
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

        protected override void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            base.OnDrawerCreatedBase(sender, e);
            _player.CreateDrawer(Transform);
            _enemy.CreateDrawer(Transform);
        }
        protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            base.OnDrawerDestroyedBase(sender, e);
            _player.DestroyDrawer(Drawer?.IsDestroyed ?? true);
            _enemy.DestroyDrawer(Drawer?.IsDestroyed ?? true);
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

        protected virtual UniTask OnStartPhaseBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (!terr.DrawersAreNull)
                Menu.WriteLogToCurrent($"-- Ход {terr.Turn} (начало) --");
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (start)");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEnemyPhaseBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (enemy)");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerPhaseBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (player)");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEndPhaseBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (!terr.DrawersAreNull)
                Menu.WriteLogToCurrent($"-- Ход {terr.Turn} (конец) --");
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (end)");
            foreach (BattleField field in terr.Fields().WithCard())
                terr.Initiations.Enqueue(field.Card.CreateInitiation());
            if (terr.Initiations.Count != 0)
                 terr.Initiations.Run();
            else terr.NextPhase();
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnNextPhaseBase(object sender, EventArgs e)
        {
            BattleArea.StopTargetAiming();
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerWonBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"{terr.TableNameDebug}: player won");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerLostBase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"{terr.TableNameDebug}: player lost");
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

        UniTask OnSideHealthSet(object sender, TableStat.PostSetArgs e)
        {
            if (e.newStatValue > 0)
                return UniTask.CompletedTask;

            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;
            BattleTerritory terr = side.Territory;

            terr._phaseCycleEnabled = false; // blocks phase cycle (to block next turn start)
            terr._initiations.OnceComplete += (s, e) =>
            {
                // unblocks phase cycle if none of the sides are dead
                if (terr._player.health > 0 && terr._enemy.health > 0)
                {
                    terr._phaseCycleEnabled = true;
                    terr.NextPhase();
                    return;
                }

                terr.SetFieldsColliders(false);
                terr.Conclude(terr._player.health > 0);
            };
            return UniTask.CompletedTask;
        }

        BattleSide GetPhaseSide()
        {
            if (_concluded || _currentPhaseCycleIndex == START_PHASE_INDEX || _currentPhaseCycleIndex == END_PHASE_INDEX)
                return null;
            return _currentPhaseCycleIndex == _playerPhaseCycleIndex ? _player : _enemy;
        }
        BattleSide GetWaitingSide()
        {
            if (_concluded || _currentPhaseCycleIndex == START_PHASE_INDEX || _currentPhaseCycleIndex == END_PHASE_INDEX)
                return null;
            return _currentPhaseCycleIndex == _playerPhaseCycleIndex ? _enemy : _player;
        }
    }
}
