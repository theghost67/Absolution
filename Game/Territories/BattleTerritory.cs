using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Menus;
using Game.Palette;
using MyBox;
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

        const int START_PHASE_INDEX = 0;
        const int END_PHASE_INDEX = 3;

        // NOTE: do NOT add handlers to OnEndPhase outside of territories classes (it will have same effect as adding to OnStartPhase)
        public ITableEventVoid OnStartPhase => _onStartPhase;
        public ITableEventVoid OnEnemyPhase => _onEnemyPhase;
        public ITableEventVoid OnPlayerPhase => _onPlayerPhase;
        public ITableEventVoid OnEndPhase => _onEndPhase;
        public ITableEventVoid OnNextPhase => _onNextPhase; // invokes before any other phase event
        public ITableEventVoid OnPlayerWon => _onPlayerWon;
        public ITableEventVoid OnPlayerLost => _onPlayerLost;

        public bool IsConcluded => _isConcluding;
        public bool PlayerMovesFirst => _playerMovesFirst;
        public bool PhaseCycleEnabled => _phaseCycleEnabled;
        public BattleInitiationQueue Initiations => _initiations;

        public BattleSide PhaseSide => GetPhaseSide();
        public BattleSide WaitingSide => GetWaitingSide();

        public int PhaseCyclesPassed => _phaseCyclesPassed;
        public int PhasesPassed => _phasesPassed;
        public int Turn => _phaseCyclesPassed + 1;

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
        readonly Dictionary<int, BattleFieldCard> _stash;
        readonly List<TableEventVoid> _phaseCycleEvents;
        readonly bool _playerMovesFirst;
        readonly int _playerPhaseCycleIndex;
        readonly string _eventsGuid;

        BattleSide _player;
        BattleSide _enemy;

        bool _isConcluding;
        bool _isConcluded;

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
            _stash = new Dictionary<int, BattleFieldCard>();

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

            _onStartPhase.Add(_eventsGuid, OnStartPhaseBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onEnemyPhase.Add(_eventsGuid, OnEnemyPhaseBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onPlayerPhase.Add(_eventsGuid, OnPlayerPhaseBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onEndPhase.Add(_eventsGuid, OnEndPhaseBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onNextPhase.Add(_eventsGuid, OnNextPhaseBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onPlayerWon.Add(_eventsGuid, OnPlayerWonBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onPlayerLost.Add(_eventsGuid, OnPlayerLostBase_TOP, TableEventVoid.TOP_PRIORITY);

            FillPhaseEventsList();
            AddOnInstantiatedAction(GetType(), typeof(BattleTerritory), () =>
            {
                _player = PlayerSideCreator();
                _enemy = EnemySideCreator();
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

            FillPhaseEventsList();
            _player = PlayerSideCloner(src._player, args);
            _enemy = EnemySideCloner(src._enemy, args);

            _stash = new();
            foreach (KeyValuePair<int, BattleFieldCard> pair in src._stash)
            {
                BattleFieldCard srcCard = pair.Value;
                BattleFieldCardCloneArgs cArgs = new((FieldCard)srcCard.Data.Clone(), null, srcCard.Side.isMe ? _player : _enemy, args);
                BattleFieldCard clnCard = (BattleFieldCard)srcCard.Clone(cArgs);
                _stash.Add(pair.Key, clnCard);
            }

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

        public bool IsStartPhase()
        {
            return _currentPhaseCycleIndex == START_PHASE_INDEX;
        }
        public bool IsPlayerPhase()
        {
            return _currentPhaseCycleIndex == (_playerMovesFirst ? 1 : 2);
        }
        public bool IsEnemyPhase()
        {
            return _currentPhaseCycleIndex == (_playerMovesFirst ? 2 : 1);
        }
        public bool IsEndPhase()
        {
            return _currentPhaseCycleIndex == END_PHASE_INDEX;
        }

        public void AddToStash(BattleFieldCard card)
        {
            _stash.Add(card.Guid, card);
        }
        public void RemoveFromStash(BattleFieldCard card)
        {
            _stash.Remove(card.Guid);
        }
        public BattleFieldCard GetFromStash(int cardGuid)
        {
            if (_stash.TryGetValue(cardGuid, out BattleFieldCard card))
                 return card;
            else return null;
        }

        public BattleSide GetOppositeOf(BattleSide side)
        {
            return _player == side ? _enemy : _player;
        }
        public async UniTask NextPhase()
        {
            Again:
            if (!_phaseCycleEnabled) return;
            if (_phaseCycleEvents.Count == 0) return;

            _currentPhaseCycleIndex++;
            _phasesPassed++;

            if (_currentPhaseCycleIndex >= _phaseCycleEvents.Count)
            {
                _currentPhaseCycleIndex = 0;
                _phaseCyclesPassed++;
            }

            await _onNextPhase.Invoke(this, EventArgs.Empty);
            await _phaseCycleEvents[_currentPhaseCycleIndex].Invoke(this, EventArgs.Empty);

            if ((IsStartPhase() && PassThroughStartPhase()) ||
                (IsEndPhase() && PassThroughEndPhase()))
                goto Again;
        }
        public async UniTask LastPhase()
        {
            int times = END_PHASE_INDEX - _currentPhaseCycleIndex;
            for (int i = 0; i < times; i++)
                await NextPhase();
        }
        public async UniTask TryConclude()
        {
            if (_isConcluding) return;
            _isConcluding = true;

            _phaseCycleEnabled = false; // blocks phase cycle (to block next turn start)
            if (!_initiations.IsRunning)
                await TryConcludeInternal();
            else _initiations.OnceComplete += (s, e) =>
            {
                TryConcludeInternal();
                if (_isConcluded) return;
                _phaseCycleEnabled = true;
            };
        }
        
        public UniTask<BattleFieldCard> PlaceFieldCard(FieldCard data, BattleField field, ITableEntrySource source)
        {
            if (data == null || field == null) 
                throw new NullReferenceException();
            if (field.Card != null)
                return UniTask.FromResult<BattleFieldCard>(null);

            BattleFieldCard card = FieldCardCreator(data, field.Side);
            if (card.Drawer != null)
            {
                card.Drawer.ColliderEnabled = false;
                card.Drawer.Alpha = 0;
                card.Drawer.transform.position = field.Drawer.transform.position;
                card.Drawer.DOFade(1, 0.5f).SetEase(Ease.InCubic).OnComplete(() => card.Drawer.ColliderEnabled = true);
            }
            return PlaceFieldCard(card, field, source);
        }
        public UniTask<BattleFloatCard> PlaceFloatCard(FloatCard data, BattleSide side, ITableEntrySource source)
        {
            if (data == null)
                throw new NullReferenceException();

            BattleFloatCard card = FloatCardCreator(data, side);
            return PlaceFloatCard(card, source);
        }

        public async UniTask<BattleFieldCard> PlaceFieldCard(BattleFieldCard card, BattleField field, ITableEntrySource source)
        {
            if (card == null || field == null)
                throw new NullReferenceException();

            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;

            await card.TryAttachToField(field, source);
            if (card.IsKilled || card.Field != null)
            {
                TableConsole.LogToFile("terr", $"{TableNameDebug}: field card placement: id: {card.Data.id}, field: {field.TableNameDebug} (by: {sourceNameDebug}).");
                return card;
            }

            card.Dispose();
            Debug.LogError("Territory field card placement failed. Field is already occupied.");
            return null;
        }
        public async UniTask<BattleFloatCard> PlaceFloatCard(BattleFloatCard card, ITableEntrySource source)
        {
            if (card == null) throw new NullReferenceException();
            if (!card.IsUsable(new TableFloatCardUseArgs(card, this)))
                throw new InvalidOperationException("Float card cannot be placed (used) on territory.");

            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;
            TableConsole.LogToFile("terr", $"{TableNameDebug}: float card placement: id: {card.Data.id} (by: {sourceNameDebug}).");
            await card.TryUse();
            return card;
        }

        public new BattleField Field(in int2 pos)
        {
            return Field(pos.x, pos.y);
        }
        public new BattleField Field(in int x, in int y)
        {
            if (x < 0 || x >= _fields.GetLength(0)) return null;
            if (y < 0 || y >= _fields.GetLength(1)) return null;
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

        protected virtual UniTask OnStartPhaseBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (!terr.DrawersAreNull)
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (start)");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEnemyPhaseBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (enemy)");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerPhaseBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (player)");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEndPhaseBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (!terr.DrawersAreNull)
            TableConsole.LogToFile("terr", $"turn: {terr.Turn} (end)");

            if (BattleInitiationQueue.IsAnyRunning)
            {
                Debug.LogError("Territory end phase event invoked while initiation queue is running.");
                return UniTask.CompletedTask;
            }

            List<BattleInitiationSendArgs> initiations = new();
            foreach (BattleField field in terr.Fields().WithCard())
                initiations.Add(field.Card.CreateInitiation());

            terr.Initiations.Enqueue(initiations);
            return terr.Initiations.Await();
        }
        protected virtual UniTask OnNextPhaseBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (!terr.DrawersAreNull)
                BattleArea.StopTargetAiming();
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerWonBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"{terr.TableNameDebug}: player won");

            terr._onStartPhase.Clear();
            terr._onEnemyPhase.Clear();
            terr._onPlayerPhase.Clear();
            terr._onEndPhase.Clear();
            terr._onNextPhase.Clear();

            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPlayerLostBase_TOP(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            TableConsole.LogToFile("terr", $"{terr.TableNameDebug}: player lost");

            terr._onStartPhase.Clear();
            terr._onEnemyPhase.Clear();
            terr._onPlayerPhase.Clear();
            terr._onEndPhase.Clear();
            terr._onNextPhase.Clear();

            return UniTask.CompletedTask;
        }

        protected virtual void OnInitiationsProcessingStartBase(object sender, EventArgs e) { }
        protected virtual void OnInitiationsProcessingEndBase(object sender, EventArgs e) { }

        // if true, will call NextPhase again after phase event was called (in NextPhase)
        protected virtual bool PassThroughStartPhase() => true;
        protected virtual bool PassThroughEndPhase() => true;

        BattleSide GetPhaseSide()
        {
            if (_isConcluded || _currentPhaseCycleIndex == START_PHASE_INDEX || _currentPhaseCycleIndex == END_PHASE_INDEX)
                return null;
            return _currentPhaseCycleIndex == _playerPhaseCycleIndex ? _player : _enemy;
        }
        BattleSide GetWaitingSide()
        {
            if (_isConcluded || _currentPhaseCycleIndex == START_PHASE_INDEX || _currentPhaseCycleIndex == END_PHASE_INDEX)
                return null;
            return _currentPhaseCycleIndex == _playerPhaseCycleIndex ? _enemy : _player;
        }

        UniTask TryConcludeInternal()
        {
            _isConcluding = false;

            bool anySideIsKilled = _player.IsKilled || _enemy.IsKilled;
            if (!anySideIsKilled)
                return UniTask.CompletedTask;
            if (_isConcluded)
                return UniTask.CompletedTask;

            _isConcluding = true;
            _isConcluded = true;

            if (_player.Health > 0)
                 return _onPlayerWon.Invoke(this, EventArgs.Empty);
            else return _onPlayerLost.Invoke(this, EventArgs.Empty);
        }
        void FillPhaseEventsList()
        {
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
        }
    }
}
