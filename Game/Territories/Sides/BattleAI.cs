using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Sleeves;
using Game.Traits;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий ИИ-действия стороны сражения (см. <see cref="BattleSide"/>).
    /// </summary>
    public sealed class BattleAI
    {
        const int TURN_DELAY_MS = 500;
        const int ITERATIONS_MAX = 64;

        readonly BattleSide _side;

        public static bool IsAnyMakingTurn => _isAnyMakingTurn;
        public bool IsMakingTurn => _isMakingTurn;
        public bool UsesAiming
        {
            get => _usesAiming;
            set
            {
                if (_isMakingTurn)
                    throw new InvalidOperationException($"Cannot change BattleAI properties when AI {nameof(IsMakingTurn)}.");
                _usesAiming = value;
            }
        }
        public PlayStyle Style
        {
            get => _style;
            set
            {
                if (_isMakingTurn)
                    throw new InvalidOperationException($"Cannot change BattleAI properties when AI {nameof(IsMakingTurn)}.");
                _style = value;
            }
        }
        private float2 SidesWeight => new(_side.Weight, _side.Opposite.Weight);

        List<string> _lastTerrLogs;
        BattleTerritory _lastTerr;
        Tween _turnTween;

        static bool _isAnyMakingTurn;
        bool _isMakingTurn;
        bool _usesAiming;
        PlayStyle _style;

        public enum PlayStyle
        {
            Balanced,
            Defensive,
            Offensive,
        }
        class CardsResultsListSet : IReadOnlyList<CardsResultsList>
        {
            public int Count => _list.Count;
            readonly List<CardsResultsList> _list;

            public CardsResultsListSet() 
            { 
                _list = new List<CardsResultsList>();
                foreach (CardCurrency currency in CardBrowser.Currencies)
                    _list.Add(new CardsResultsList(currency));
            }

            public CardsResultsList this[CardCurrency currency] => _list.First(c => c.currency == currency);
            public CardsResultsList this[int index] => _list[index];

            public bool Contains(CardCurrency listCurrency)
            {
                return _list.Any(l => l.currency == listCurrency);
            }
            public void Add(CardsResultsList list)
            {
                _list.Add(list);
            }
            public void Remove(CardsResultsList list) 
            {
                _list.Remove(list);
            }
            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }
            public void Clear()
            {
                _list.Clear();
            }

            IEnumerator<CardsResultsList> IEnumerable<CardsResultsList>.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }
        class CardsResultsList : IReadOnlyList<IBattleWeightResult>, IEquatable<CardsResultsList>
        {
            public int Count => _list.Count;
            public readonly CardCurrency currency;
            readonly List<IBattleWeightResult> _list;

            public CardsResultsList(CardCurrency currency)
            {
                this.currency = currency;
                this._list = new List<IBattleWeightResult>(CardDeck.LIMIT);
            }
            public bool Equals(CardsResultsList other)
            {
                return currency == other.currency;
            }

            public IBattleWeightResult this[int index] => _list[index];

            public bool Contains(IBattleWeightResult result)
            {
                return _list.Contains(result);
            }
            public void Add(IBattleWeightResult result)
            {
                _list.Add(result);
            }
            public void Remove(IBattleWeightResult result)
            {
                _list.Remove(result);
            }
            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }
            public void Clear()
            {
                _list.Clear();
            }

            IEnumerator<IBattleWeightResult> IEnumerable<IBattleWeightResult>.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public BattleAI(BattleSide side) 
        {
            _lastTerrLogs = new List<string>();
            _side = side;
            _style = PlayStyle.Defensive;
        }
        public async UniTask MakeTurn()
        {
            if (_isMakingTurn || _side.Territory.PhaseSide != _side)
            {
                Debug.LogError($"can't make a turn because it's not AI's place phase or already making turn.");
                return;
            }

            _isMakingTurn = true;
            _isAnyMakingTurn = true;
            _turnTween = DOVirtual.DelayedCall(5, () =>
            {
                _isMakingTurn = false;
                _lastTerr?.Dispose();
            });

            try
            {
                TableConsole.LogToFile("ai", $"TURN STARTED");
                await MakeTurn_Cards();
                await MakeTurn_Traits();
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogException(ex);
                _lastTerr?.Dispose();
            }

            TableConsole.LogToFile("ai", $"TURN ENDED");
            _turnTween.Kill();
            _isMakingTurn = false;
            _isAnyMakingTurn = false;
            _side.Territory.NextPhase();
        }

        async UniTask MakeTurn_Cards()
        {
            int iterations = 0;
            CardsResultsListSet resultsSet = new();

            Start:
            await TableEventManager.AwaitAnyEvents();
            await UniTask.Delay(TURN_DELAY_MS);

            if (iterations++ > ITERATIONS_MAX)
            {
                Debug.LogError("too many iterations.");
                return;
            }
            float2 sidesWeight = SidesWeight;
            IBattleWeightResult noPlaceResult = GetNoPlacementResult(sidesWeight);
            IBattleSleeveCard[] sleeveCards = ((IEnumerable<IBattleSleeveCard>)_side.Sleeve).ToArray();
            IBattleSleeveCard[] availableCards = sleeveCards.Where(c => _side.CanAfford(c)).ToArray();
            TableConsole.LogToFile("ai", $"CARD PLACE ITERATION {iterations}");
            if (availableCards.Length == 0)
            {
                TableConsole.LogToFile("ai", $"CANNOT AFFORD ANY CARD (count: {sleeveCards.Length})");
                return;
            }

            await UniTask.SwitchToThreadPool();
            BeginWritingLastTerrLogs();

            for (int i = 0; i < sleeveCards.Length; i++)
            {
                IBattleSleeveCard card = sleeveCards[i];
                CardCurrency currency = card.Data.price.currency;
                if (!resultsSet.Contains(currency)) continue; // CardCurrency could be skipped

                IBattleWeightResult result;
                if (card.Data.isField)
                {
                    BattleFieldCardWeightResult fieldRes = GetBestFieldCardPlaceResult((BattleFieldCard)card, sidesWeight);
                    if (fieldRes != null)
                         TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: abs: {fieldRes.WeightDeltaAbs}, rel: {fieldRes.WeightDeltaRel}, field: {fieldRes.Field.TableNameDebug}");
                    else TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: null (no fields or cannot afford)");
                    result = fieldRes;
                }
                else
                {
                    BattleFloatCardWeightResult floatRes = GetBestFloatCardUseResult((BattleFloatCard)card, sidesWeight);
                    if (floatRes != null)
                         TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: abs: {floatRes.WeightDeltaAbs}, rel: {floatRes.WeightDeltaRel}");
                    else TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: null (cannot afford)");
                    result = floatRes;
                }
                if (result != null && result.WeightDeltaAbs != 0 && result.WeightDeltaAbs > noPlaceResult.WeightDeltaAbs)
                    resultsSet[currency].Add(result);
            };

            StopWritingLastTerrLogs(forCards: true);
            await UniTask.SwitchToMainThread();

            for (int i = 0; i < resultsSet.Count; i++) // iterates for each CardCurrency in the game
            {
                CardsResultsList results = resultsSet[i];
                PickResultAgain:
                IBattleWeightResult result = GetBestResult(results, out int index);
                if (result != null)
                    results.RemoveAt(index);
                else
                {
                    results.Clear();
                    continue;
                }

                IBattleSleeveCard resultCard = (IBattleSleeveCard)result.Entity;
                BattleField resultField = null;
                if (!_side.CanAfford(resultCard))
                {
                    resultsSet.Remove(results); // skips all cards with the same price type (waits for next turn)
                    i--; continue;
                }

                TableConsole.LogToFile("ai", $"CARD PLACEMENT BEGINS: {result.Entity.TableNameDebug} ");
                if (result is BattleFieldCardWeightResult resultOfFieldCard)
                    resultField = resultOfFieldCard.Field;
                else
                {
                    BattleFloatCardWeightResult resultOfFloatCard = (BattleFloatCardWeightResult)result;
                    BattleFloatCard floatCard = resultOfFloatCard.Entity;
                    FloatCard floatCardData = floatCard.Data;
                    TableFloatCardUseArgs floatCardUseArgs = new(floatCard, _side.Territory);
                    if (!floatCardData.IsUsable(floatCardUseArgs) || !floatCardData.Threshold.WeightIsEnough(resultOfFloatCard))
                        goto PickResultAgain;
                }
                resultCard.TryDropOn(resultField);
                TableConsole.LogToFile("ai", $"CARD PLACEMENT ENDS: {result.Entity.TableNameDebug}");
                goto End;
            }

            End:
            int resultsCount = 0;
            foreach (CardsResultsList list in resultsSet)
                resultsCount += list.Count;
            resultsSet.Clear();
            if (resultsCount != 0)
                goto Start;
        }
        async UniTask MakeTurn_Traits()
        {
            int iterations = 0;
            bool lockActive = false;

            Start:
            await TableEventManager.AwaitAnyEvents();
            await UniTask.Delay(TURN_DELAY_MS);

            if (iterations++ > ITERATIONS_MAX)
            {
                Debug.LogError("too many iterations.");
                return;
            }
            float2 sidesWeight = SidesWeight;
            IBattleWeightResult noUseResult = GetNoPlacementResult(sidesWeight);
            TableConsole.LogToFile("ai", $"TRAIT USE ITERATION {iterations}");

            // adds active traits from sleeve cards and cards placed on side's fields to list
            List<BattleActiveTraitListElement> elements = new();
            IEnumerable<IBattleSleeveCard> sleeveCards = _side.Sleeve;
            IEnumerable<BattleFieldCard> cards = _side.Fields().WithCard().Select(f => f.Card).Concat(sleeveCards.Where(c => c is BattleFieldCard).Cast<BattleFieldCard>());
            foreach (BattleActiveTraitListElement element in cards.SelectMany<BattleFieldCard, BattleActiveTraitListElement>(f => f.Traits.Actives))
                elements.Add(element);
            if (elements.Count == 0)
            {
                TableConsole.LogToFile("ai", $"NO TRAITS FOUND");
                return;
            }

            await UniTask.SwitchToThreadPool();
            BeginWritingLastTerrLogs();

            List<BattleActiveTraitWeightResult> results = new();
            foreach (BattleActiveTraitListElement element in elements)
            {
                BattleActiveTraitWeightResult result = GetBestActiveTraitUseResult(element.Trait, sidesWeight);
                BattleWeight threshold = element.Trait.Data.WeightDeltaUseThreshold(result);

                if (result == null)
                    TableConsole.LogToFile("vrt", $"active trait: name: {element.Trait.TableNameDebug}, threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: null");
                else if (result.Field == null)
                     TableConsole.LogToFile("vrt", $"active trait: name: {element.Trait.TableNameDebug}, threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: abs: {result.WeightDeltaAbs}, rel: {result.WeightDeltaRel}, target: null");
                else TableConsole.LogToFile("vrt", $"active trait: name: {element.Trait.TableNameDebug}, threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: abs: {result.WeightDeltaAbs}, rel: {result.WeightDeltaRel}, target: {result.Field.TableNameDebug}");

                if (result != null && result.WeightDeltaAbs != 0 && result.WeightDeltaAbs > noUseResult.WeightDeltaAbs) 
                    results.Add(result);
            }

            StopWritingLastTerrLogs(forCards: false);
            await UniTask.SwitchToMainThread();

            PickResultAgain:
            BattleActiveTraitWeightResult bestResult = GetBestResult(results, out int index);
            if (bestResult == null) return;
            results.RemoveAt(index);

            BattleActiveTrait activeTrait = bestResult.Entity;
            ActiveTrait activeTraitData = activeTrait.Data;
            TableActiveTraitUseArgs activeTraitUseArgs = new(activeTrait, bestResult.Field);
            if (!activeTraitData.IsUsable(activeTraitUseArgs) || !activeTraitData.Threshold.WeightIsEnough(bestResult))
                goto PickResultAgain;

            await activeTrait.TryUse(bestResult.Field);
            goto Start;
        }

        IBattleWeightResult GetNoPlacementResult(float2 sidesWeight)
        {
            BeginWritingLastTerrLogs();
            UseVirtual(null, sidesWeight, out float2 deltas);
            StopWritingLastTerrLogsForNoPlacement();
            BattleFloatCardWeightResult result = new(null, deltas[0], deltas[1]);
            TableConsole.LogToFile("ai", $"NOP result: delta abs: {result.WeightDeltaAbs}.");
            return result;
        }
        BattleFieldCardWeightResult GetBestFieldCardPlaceResult(BattleFieldCard card, float2 sidesWeight)
        {
            if (card.Side != _side)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            int2[] possibleFieldsPos = card.Side.Fields().WithoutCard().Select(f => f.pos).ToArray(); // TODO[in next updates]: check if card trait has Attr.ALLOWS_TO_STACK_CARDS
            BattleFieldCardWeightResult[] results = new BattleFieldCardWeightResult[possibleFieldsPos.Length];
            BattleTerritory srcTerritory = _side.Territory;

            for (int i = 0; i < possibleFieldsPos.Length; i++)
            {
                int2 fieldPos = possibleFieldsPos[i];
                BattleTerritory terrClone = CloneTerritory(_side.Territory, out BattleTerritoryCloneArgs terrCArgs);

                if (srcTerritory.GetType() != terrClone.GetType())
                    throw new Exception($"{srcTerritory.GetType()} must implement {nameof(ICloneableWithArgs)} in order to use BattleAI. " +
                                        $"Also consider implementing this interface in all classes used in territory.");

                BattleSide sideClone = _side.isMe ? terrClone.Player : terrClone.Enemy;
                FieldCard dataClone = sideClone.Deck.fieldCards[card.Data.Guid];
                BattleField fieldClone = terrClone.Field(fieldPos.x, fieldPos.y);
                BattleField cardFieldClone = card.Field == null ? null : terrClone.Field(card.Field.pos);
                BattleFieldCardCloneArgs cardCArgs = new(dataClone, cardFieldClone, sideClone, terrCArgs);
                BattleFieldCard cardClone = (BattleFieldCard)card.Clone(cardCArgs);
                int cardCloneHealth = cardClone.health;

                terrClone.PlaceFieldCard(cardClone, fieldClone, sideClone);
                terrClone.LastPhase();

                BattleSide sideCloneOpposite = sideClone.Opposite;
                float2 weightsWeightAfterTurn = new(sideClone.Weight, sideCloneOpposite.Weight);
                float2 weightDelta = CalculateWeightDelta(sidesWeight, weightsWeightAfterTurn, sideClone, sideCloneOpposite);
                results[i] = new BattleFieldCardWeightResult(card, srcTerritory.Field(fieldPos), weightDelta[0], weightDelta[1]);

                terrClone.Dispose();
            }

            BattleFieldCardWeightResult bestResult = GetBestResult(results, out _);
            return bestResult;
        }
        BattleFloatCardWeightResult GetBestFloatCardUseResult(BattleFloatCard card, float2 sidesWeight)
        {
            if (card.Side != _side)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            // there's no point in cloning if object is not usable
            if (!card.IsUsable(new TableFloatCardUseArgs(card, card.Territory)))
                return null;

            void UseAction(BattleTerritory terr) => ((BattleFloatCard)card.Finder.FindInBattle(terr)).TryUse();
            UseVirtual(UseAction, sidesWeight, out float2 deltas);

            BattleFloatCardWeightResult bestResult = new(card, deltas[0], deltas[1]); // single because cannot be placed on field (no difference)
            return bestResult;
        }
        BattleActiveTraitWeightResult GetBestActiveTraitUseResult(BattleActiveTrait trait, float2 sidesWeight)
        {
            if (trait.Side != _side)
                throw new InvalidOperationException("Provided trait does not belong to this side.");

            List<BattleField> potTargets = new();
            List<BattleActiveTraitWeightResult> results = new();
            potTargets.AddRange(trait.Area.PotentialTargets());
            if (potTargets.Count == 0) potTargets.Add(null);

            foreach (BattleField target in potTargets)
            {
                // there's no point in cloning if object is not usable
                if (!trait.IsUsable(new TableActiveTraitUseArgs(trait, target)))
                    continue;
                void UseAction(BattleTerritory terr)
                {
                    BattleActiveTrait traitClone = (BattleActiveTrait)trait.Finder.FindInBattle(terr);
                    BattleField targetClone = (BattleField)target.Finder.FindInBattle(terr);
                    traitClone.TryUse(targetClone);
                }
                UseVirtual(UseAction, sidesWeight, out float2 deltas);

                BattleActiveTraitWeightResult result = new(trait, target, deltas[0], deltas[1]);
                results.Add(result);
            }
            BattleActiveTraitWeightResult bestResult = GetBestResult(results, out _);
            return bestResult;
        }
        static T GetBestResult<T>(IReadOnlyCollection<T> results, out int index) where T : class, IBattleWeightResult
        {
            if (results.Count == 0)
            {
                index = -1;
                return null;
            }
            List<T> positiveResults = new();
            List<T> negativeResults = new();
            foreach (T result in results)
            {
                if (result.WeightDeltaAbs < 0)
                     negativeResults.Add(result);
                else positiveResults.Add(result);
            }
            if (positiveResults.Count != 0)
            {
                index = positiveResults.GetWeightedRandomIndex(r => Mathf.Pow(r.WeightDeltaAbs, 3));
                if (index != -1)
                     return positiveResults[index];
                else return null;
            }
            else if (negativeResults.Count != 0)
            {
                index = negativeResults.GetWeightedRandomIndex(r => Mathf.Pow(-r.WeightDeltaAbs, 3));
                if (index != -1)
                    return negativeResults[index];
                else return null;
            }
            else
            {
                index = -1;
                return null;
            }
        }

        void UseVirtual(Action<BattleTerritory>? useFunc, float2 sidesWeight, out float2 deltas)
        {
            BattleTerritory terrClone = CloneTerritory(_side.Territory, out _);
            BattleSide sideClone = _side.isMe ? terrClone.Player : terrClone.Enemy;

            useFunc?.Invoke(terrClone);
            terrClone.LastPhase();

            BattleSide sideCloneOpposite = sideClone.Opposite;
            float2 weightsWeightAfterTurn = new(sideClone.Weight, sideCloneOpposite.Weight);

            deltas = CalculateWeightDelta(sidesWeight, weightsWeightAfterTurn, sideClone, sideCloneOpposite);
            terrClone.Dispose();
        }
        float2 CalculateWeightDelta(float2 sidesStartWeight, float2 sidesEndWeight, BattleSide thisSideAfterTurn, BattleSide oppoSideAfterTurn)
        {
            // before/after turn
            float thisSideWeightBefore = ThisSideWeight_ScaledByStyle(_side, sidesStartWeight.x);     
            float thisSideWeightAfter  = ThisSideWeight_ScaledByStyle(thisSideAfterTurn, sidesEndWeight.x);

            float oppoSideWeightBefore = OppoSideWeight_ScaledByStyle(_side.Opposite, sidesStartWeight.y);
            float oppoSideWeightAfter  = OppoSideWeight_ScaledByStyle(oppoSideAfterTurn, sidesEndWeight.y);

            // if sidesStartWeight sum is 200 and weightDeltaAbs is 40,
            // weightDeltaRel equals to (40 / 200) = 0.2
            float weightDeltaAbs = (thisSideWeightAfter - thisSideWeightBefore) - (oppoSideWeightAfter - oppoSideWeightBefore);
            float weightDeltaRel = weightDeltaAbs / (thisSideWeightBefore + oppoSideWeightBefore);

            return new float2(weightDeltaAbs, weightDeltaRel);
        }

        float ThisSideWeight_ScaledByStyle(BattleSide side, float weight)
        {
            int gold = side.gold;
            int ether = side.ether;
            float goldScale  = gold  <= 0 ? 1 : (float)Math.Log(gold + Math.E);
            float etherScale = ether <= 0 ? 1 : ((float)Math.Log(ether + Math.E)) / 2f + 0.5f;

            weight *= goldScale * etherScale;
            if (_style == PlayStyle.Defensive)
                 return weight * 2;
            else return weight;
        }
        float OppoSideWeight_ScaledByStyle(BattleSide side, float weight)
        {
            if (_style == PlayStyle.Offensive)
                 return weight * 2;
            else return weight;
        }

        void BeginWritingLastTerrLogs()
        {
            TableConsole.OnLogToFile += OnConsoleLogToFile;
        }
        void StopWritingLastTerrLogs(bool forCards)
        {
            TableConsole.OnLogToFile -= OnConsoleLogToFile;
            TableConsole.LogToFile("ai", $"LAST AI TERRITORY CLONE LOGS START ({(forCards ? "CARDS" : "TRAITS")})");
            TableConsole.LogToFile("ai", _lastTerrLogs);
            TableConsole.LogToFile("ai", $"LAST AI TERRITORY CLONE LOGS END ({(forCards ? "CARDS" : "TRAITS")})");
            _lastTerrLogs.Clear();
        }
        void StopWritingLastTerrLogsForNoPlacement()
        {
            TableConsole.OnLogToFile -= OnConsoleLogToFile;
            TableConsole.LogToFile("ai", $"NOP AI TERRITORY CLONE LOGS START");
            TableConsole.LogToFile("ai", _lastTerrLogs);
            TableConsole.LogToFile("ai", $"NOP AI TERRITORY CLONE LOGS END");
            _lastTerrLogs.Clear();
        }

        bool OnConsoleLogToFile(string text)
        {
            if (text.StartsWith(nameof(BattleAI)))
                return false;
            _lastTerrLogs.Add(text);
            return true;
        }
        BattleTerritory CloneTerritory(BattleTerritory src, out BattleTerritoryCloneArgs cArgs)
        {
            _lastTerrLogs.Clear();
            cArgs = new BattleTerritoryCloneArgs();
            return _lastTerr = (BattleTerritory)src.Clone(cArgs);
        }
    }
}
