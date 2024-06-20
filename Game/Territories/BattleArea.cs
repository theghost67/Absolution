using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Представляет область действия весомой сущности (см. <see cref="IBattleWeighty"/>) с отслеживанием целей на территории карты во время сражения.
    /// </summary>
    public sealed class BattleArea : ICloneableWithArgs, IDisposable
    {
        public IIdEventVoidAsync<BattleFieldCard> OnCardSeen => _onCardSeen; // potential target detection
        public IIdEventVoidAsync<BattleFieldCard> OnCardUnseen => _onCardUnseen;

        public static bool IsAnyAiming => _isAnyAiming;
        public bool IsAiming => _isAiming;
        public bool HasNoAim => Range.potential.targetIsSingle;

        public BattleRange Range => observer.Range;
        public BattleField AimedTarget => _aimedTarget;

        public readonly BattleFieldCard observingPoint;
        public readonly IBattleWeighty observer;

        static BattleArea _currentlyAimingArea;
        static bool _isAnyAiming;

        readonly TableEventVoid<BattleFieldCard> _onCardSeen;
        readonly TableEventVoid<BattleFieldCard> _onCardUnseen;

        bool _isAiming;
        bool _observeTargets;

        List<BattleFieldCard> _observingCards;
        BattleField _aimedTarget;
        BattleField[] _aimedTargetSplash;
        BattleField[] _potentialTargets;
        BattleField[][] _possibleTargets;

        Func<BattleField, bool> _aimFilter;
        Action<BattleField> _onAimFinish;
        Action _onAimCancel;

        public BattleArea(IBattleWeighty observer, BattleFieldCard observingPoint)
        {
            this.observer = observer;
            this.observingPoint = observingPoint;

            _onCardSeen = new TableEventVoid<BattleFieldCard>();
            _onCardUnseen = new TableEventVoid<BattleFieldCard>();

            _observingCards = new List<BattleFieldCard>(TableTerritory.MAX_SIZE);
            _aimedTargetSplash = Array.Empty<BattleField>();
            _potentialTargets = Array.Empty<BattleField>();
            _possibleTargets = Array.Empty<BattleField[]>();
        }
        private BattleArea(BattleArea src, BattleAreaCloneArgs args)
        {
            observer = args.srcAreaObserverClone;
            observingPoint = args.srcAreaObservingPointClone;

            _onCardSeen = (TableEventVoid<BattleFieldCard>)src._onCardSeen.Clone();
            _onCardUnseen = (TableEventVoid<BattleFieldCard>)src._onCardUnseen.Clone();

            _observingCards = new List<BattleFieldCard>(TableTerritory.MAX_SIZE);
            _aimedTargetSplash = Array.Empty<BattleField>();
            _potentialTargets = Array.Empty<BattleField>();
            _possibleTargets = Array.Empty<BattleField[]>();

            _observeTargets = src._observeTargets;
            args.terrCArgs.OnTerritoryReady += terr => Clone_OnTerrFieldsReady(src, (BattleTerritory)terr);
        }

        public void Dispose()
        {
            CancelTargetAiming();
            _observeTargets = false;
            _observingCards.Clear();
            _aimedTargetSplash = null;
            _potentialTargets = null;
            _possibleTargets = null;
        }
        public object Clone(CloneArgs args)
        {
            if (args is BattleAreaCloneArgs cArgs)
                 return new BattleArea(this, cArgs);
            else return null;
        }

        public static void StopTargetAiming()
        {
            _currentlyAimingArea?.CancelTargetAiming();
        }
        public async UniTask SetObserveTargets(bool value)
        {
            if (_observeTargets == value)
                return;

            _observeTargets = value;
            if (_observeTargets)
            {
                if (observingPoint.Field == null)
                    throw new Exception($"{nameof(BattleArea)}: SetObserveTargets(true) can be used only when observingPoint is attached to a field.");

                await EnableContiniousObserving();
                observingPoint.OnFieldPostAttached.Add(OnObservingPointAttachedToField);
            }
            else
            {
                await DisableContiniousObserving();
                observingPoint.OnFieldPostAttached.Remove(OnObservingPointAttachedToField);
            }
        }
        public void SelectTargetsByWeight()
        {
            int length = _possibleTargets.Length;
            if (length == 0 || !CanObserve()) return;
            if (length == 1)
            {
                _aimedTarget = _potentialTargets[0];
                _aimedTargetSplash = _possibleTargets[0];
                return;
            }

            float[] weights = new float[length];
            for (int i = 0; i < length; i++)
            {
                BattleField[] possibleTarget = _possibleTargets[i];
                foreach (BattleField splashTarget in possibleTarget)
                {
                    if (splashTarget.Card == null) continue;
                    BattleFieldCard splashTargetCard = splashTarget.Card;
                    BattleWeight splashTargetWeight = splashTargetCard.Weight;
                    weights[i] += BattleWeight.Float(splashTargetCard.Side.health, splashTargetWeight);
                }
            }

            int maxWeightIndex = 0;
            for (int i = 1; i < length; i++)
            {
                if (weights[i] > weights[maxWeightIndex])
                    maxWeightIndex = i;
            }

            _aimedTarget = _potentialTargets[maxWeightIndex];
            _aimedTargetSplash = _possibleTargets[maxWeightIndex];
        }
        public bool HasTarget(BattleField target)
        {
            foreach (BattleField[] possibleTarget in _possibleTargets)
            {
                foreach (BattleField splashTarget in possibleTarget)
                {
                    if (splashTarget.pos.Equals(target.pos))
                        return true;
                }
            }
            return false;
        }

        public void StartTargetAiming(Func<BattleField, bool> filter, Action<BattleField> onFinish = null, bool canCancel = true)
        {
            if (_isAnyAiming) return; // throw new InvalidOperationException("Aiming already started. There can be only one area with active aiming.");
            if (_potentialTargets.Length == 0) return; // throw new NullReferenceException($"Fixed potential targets are empty, so it's impossible to aim. Consider setting different {nameof(BattleRange)}.");

            if (HasNoAim)
            {
                _aimedTarget = _potentialTargets.First();
                _aimedTargetSplash = GetSplashTargetsOf(_aimedTarget);
                onFinish?.Invoke(_aimedTarget);
                return;
            }
            else _aimedTargetSplash = Array.Empty<BattleField>();

            Pointer.Type = PointerType.Aiming;
            observingPoint.Territory.SetCardsColliders(false);
            _isAnyAiming = true;
            _isAiming = true;
            _currentlyAimingArea = this;

            _aimFilter = filter;
            _onAimFinish = onFinish;
            _onAimCancel = null;

            foreach (BattleField field in _potentialTargets)
                SetFieldAimEvents(field);

            if (canCancel)
                Global.OnUpdate += TargetAimUpdate;
        }
        public void CancelTargetAiming()
        {
            if (!_isAiming) return;
            _onAimCancel();

            Pointer.Type = PointerType.Normal;
            observingPoint.Territory.SetCardsColliders(true);
            _isAnyAiming = false;
            _isAiming = false;
            _currentlyAimingArea = null;
        }

        public void CreateTargetsHighlight()
        {
            if (Range.potential.targetIsSingle)
            {
                foreach (BattleField[] targets in _possibleTargets)
                {
                    foreach (BattleField target in targets)
                        target.Drawer.AnimShowCovering();
                }
            }
            else
            {
                foreach (var field in _potentialTargets)
                    field.Drawer.AnimShowOutline();
            }
        }
        public void DestroyTargetsHighlight()
        {
            if (Range.potential.targetIsSingle)
            {
                foreach (BattleField[] targets in _possibleTargets)
                {
                    foreach (BattleField target in targets)
                        target.Drawer.AnimHideCovering();
                }
            }
            else
            {
                foreach (var field in _potentialTargets)
                    field.Drawer.AnimHideOutline();
            }
        }

        public IEnumerable<BattleField> AimedTargetSplash()
        {
            if (_aimedTarget == null)
                yield break;

            // returns aimed target splash targets
            foreach (BattleField field in _aimedTargetSplash)
                yield return field;
        }
        public IEnumerable<BattleField> PotentialTargets()
        {
            if (!CanObserve())
                yield break;

            // returns available targets to aim
            foreach (BattleField field in _potentialTargets)
                yield return field;
        }
        public IEnumerable<BattleField> PossibleTargets(int index = 0)
        {
            if (!CanObserve())
                yield break;

            // returns all splash targets of each available target
            foreach (BattleField field in _possibleTargets[index])
                yield return field;
        }

        async UniTask EnableContiniousObserving()
        {
            _potentialTargets = GetPotentialTargetsOf(observingPoint.Field);
            if (Range.potential.targetIsSingle)
            {
                _aimedTarget = _potentialTargets.First();
                _possibleTargets = new BattleField[1][];
                _possibleTargets[0] = GetSplashTargetsOf(_aimedTarget);
            }
            else
            {
                _possibleTargets = new BattleField[_potentialTargets.Length][];
                for (int i = 0; i < _potentialTargets.Length; i++)
                {
                    BattleField potentialTarget = _potentialTargets[i];
                    _possibleTargets[i] = GetSplashTargetsOf(potentialTarget);
                }
            }

            foreach (BattleField field in observingPoint.Territory.Fields().WithCard())
                await CheckObserveStateFor(field.Card);

            observingPoint.Territory.OnCardAttachedToField.Add(OnAnyCardAttachedToField, Range.priority);
        }
        async UniTask DisableContiniousObserving()
        {
            foreach (BattleFieldCard target in _observingCards)
                await _onCardUnseen.Invoke(this, target);

            observingPoint.Territory.OnCardDetatchedFromField.Remove(OnAnyCardAttachedToField);

            _observingCards = new List<BattleFieldCard>(TableTerritory.MAX_SIZE);
            _aimedTargetSplash = Array.Empty<BattleField>();
            _potentialTargets = Array.Empty<BattleField>();
            _possibleTargets = Array.Empty<BattleField[]>();
        }

        bool CanObserve()
        {
            return observingPoint.Field != null && _observeTargets;
        }
        async UniTask CheckObserveStateFor(BattleFieldCard target)
        {
            bool wasInRange = _observingCards.Contains(target);
            bool isInRange = _potentialTargets.Contains(target.Field);

            if (isInRange == wasInRange) return;
            if (isInRange)
            {
                _observingCards.Add(target);
                await observer.AnimCardSeen(target);
                await _onCardSeen.Invoke(this, target);
            }
            else
            {
                _observingCards.Remove(target);
                await observer.AnimCardUnseen(target);
                await _onCardUnseen.Invoke(this, target);
            }
        }

        void SetFieldAimEvents(BattleField field)
        {
            BattleField[] splashTargets = GetSplashTargetsOf(field);
            BattleFieldDrawer fieldDrawer = field.Drawer;

            void OnMouseEnter(object sender, DrawerMouseEventArgs e)
            {
                fieldDrawer.AnimShowSelection();
                foreach (BattleField splashField in splashTargets)
                    splashField.Drawer.AnimShowCovering();
            };
            void OnMouseLeave(object sender, DrawerMouseEventArgs e)
            {
                fieldDrawer.AnimHideSelection();
                foreach (BattleField splashField in splashTargets)
                    splashField.Drawer.AnimHideCovering();
            }
            void OnMouseClick(object sender, DrawerMouseEventArgs e)
            {
                if (!_aimFilter(field))
                {
                    fieldDrawer.CreateTextAsSpeech("НЕВЕРНАЯ ЦЕЛЬ", Color.red);
                    return;
                }
                OnMouseLeave(sender, e);
                FinishAim(field);
            }

            fieldDrawer.AnimShowOutline();
            fieldDrawer.OnMouseEnter += OnMouseEnter;
            fieldDrawer.OnMouseLeave += OnMouseLeave;
            fieldDrawer.OnMouseClickLeft += OnMouseClick;

            _onAimCancel += () =>
            {
                fieldDrawer.AnimHideSelection();
                fieldDrawer.AnimHideOutline();
                fieldDrawer.AnimHideCovering();

                fieldDrawer.OnMouseEnter -= OnMouseEnter;
                fieldDrawer.OnMouseLeave -= OnMouseLeave;
                fieldDrawer.OnMouseClickLeft -= OnMouseClick;
            };
        }
        void FinishAim(BattleField target)
        {
            CancelTargetAiming();
            _aimedTarget = target;
            _aimedTargetSplash = GetSplashTargetsOf(_aimedTarget);
            _onAimFinish(target);
        }
        void TargetAimUpdate()
        {
            if (Input.GetMouseButtonDown(1))
            {
                CancelTargetAiming();
                Global.OnUpdate -= TargetAimUpdate;
            }
        }

        BattleField[] GetSplashTargetsOf(BattleField center)
        {
            return observingPoint.Territory.Fields(center.pos, Range.splash).ToArray();
        }
        BattleField[] GetPotentialTargetsOf(BattleField center)
        {
            return observingPoint.Territory.Fields(center.pos, Range.potential).ToArray();
        }

        UniTask OnObservingPointAttachedToField(object sender, EventArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            IBattleWeighty observer = (IBattleWeighty)this.observer.Finder.FindInBattle(card.Territory);
            if (observer == null) return UniTask.CompletedTask;

            BattleArea area = observer.Area;
            if (area.CanObserve())
                 return area.EnableContiniousObserving();
            else return UniTask.CompletedTask;
        }
        UniTask OnAnyCardAttachedToField(object sender, TableField field)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleWeighty observer = (IBattleWeighty)this.observer.Finder.FindInBattle(terr);
            if (observer == null) return UniTask.CompletedTask;

            BattleArea area = observer.Area;
            if (area.CanObserve())
                 return area.CheckObserveStateFor((BattleFieldCard)field.Card);
            else return UniTask.CompletedTask;
        }

        // finds all fields after they were cloned
        void Clone_OnTerrFieldsReady(BattleArea src, BattleTerritory terr)
        {
            if (observingPoint.Field == null) return;
            //if (src.observer is Cards.IBattleCard card)
            //     Debug.Log($"Finding targets of BattleArea that belongs to: {card.Data.name}");
            //else if (src.observer is Traits.IBattleTrait trait)
            //    Debug.Log($"Finding targets of BattleArea that belongs to: {trait.Owner.Data.name}->{trait.Data.name}");

            List<BattleFieldCard> srcObservingCards = src._observingCards;
            BattleField[] srcPotentialTargets = src._potentialTargets;
            BattleField[][] srcPossibleTargets = src._possibleTargets;

            if (srcPotentialTargets == null)
            {
                Debug.LogError($"src (BattleArea) potential targets were null.\nObserver name: {src.observer.TableName}, Observing point name: {src.observingPoint.TableName}.");
                Debug.LogError(BattleAI.LastTerritoryLog);
                return;
            }

            _observingCards = new List<BattleFieldCard>(srcObservingCards.Count);
            _potentialTargets = new BattleField[srcPotentialTargets.Length];
            _possibleTargets = new BattleField[srcPossibleTargets.Length][];
            if (src._aimedTarget != null)
            {
                _aimedTarget = (BattleField)src._aimedTarget.Finder.FindInBattle(terr);
                _aimedTargetSplash = GetSplashTargetsOf(_aimedTarget);
            }

            for (int i = 0; i < srcObservingCards.Count; i++)
                _observingCards.Add((BattleFieldCard)srcObservingCards[i].Finder.FindInBattle(terr));
            for (int i = 0; i < srcPotentialTargets.Length; i++)
                _potentialTargets[i] = (BattleField)srcPotentialTargets[i].Finder.FindInBattle(terr);
            for (int i = 0; i < srcPossibleTargets.Length; i++)
            {
                BattleField[] srcPossibleTargetArr = srcPossibleTargets[i];
                BattleField[] clnPossibleTargetArr = new BattleField[srcPossibleTargetArr.Length];
                _possibleTargets[i] = clnPossibleTargetArr;

                for (int j = 0; j < srcPossibleTargetArr.Length; j++)
                    clnPossibleTargetArr[j] = (BattleField)srcPossibleTargetArr[j].Finder.FindInBattle(terr);
            }
        }
    }
}
