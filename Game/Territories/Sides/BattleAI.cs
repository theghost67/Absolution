using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Sleeves;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий ИИ-действия стороны сражения (см. <see cref="BattleSide"/>).
    /// </summary>
    public sealed class BattleAI
    {
        const int TURN_DELAY_MS = 500;
        const int ITERATIONS_MAX = 128;

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
        List<string> _lastTerrCloneLogs;

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
            _lastTerrCloneLogs = new List<string>();
            _side = side;
            _style = PlayStyle.Defensive;
        }

        public async UniTask MakeTurn()
        {
            if (_isAnyMakingTurn || _side.Territory.PhaseSide != _side)
            {
                Debug.LogError($"Can't make a turn as an AI. Check all conditions before calling this function.");
                return;
            }

            int eventsCount = TableEventManager.CountAll();
            TableConsole.LogToFile("ai", $"TURN STARTED");
            _isMakingTurn = true;
            _isAnyMakingTurn = true;
            try
            {

                await MakeTurn_Await(eventsCount);
                await MakeTurn_Cards(eventsCount);
                await MakeTurn_Await(eventsCount);
                await MakeTurn_Traits(eventsCount);
                await MakeTurn_Await(eventsCount);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _isMakingTurn = false;
                _isAnyMakingTurn = false;
                TableConsole.LogToFile("ai", $"TURN ENDED");
                await _side.Territory.NextPhase();
                await MakeTurn_Await(eventsCount);
            }
        }
        public static IBattleWeightResult GetBestUseResult(IBattleObject obj, params int[] excludedWeights)
        {
            BattleSide side = obj.Side;
            float2 sidesWeight = GetSidesWeight(side, excludedWeights);
            return obj switch
            {
                BattleFieldCard => GetBestFieldCardPlaceResult((BattleFieldCard)obj, side, sidesWeight),
                BattleFloatCard => GetBestFloatCardUseResult((BattleFloatCard)obj, side, sidesWeight),
                BattleActiveTrait => GetBestActiveTraitUseResult((BattleActiveTrait)obj, side, sidesWeight),
                _ => throw new NotSupportedException()
            };
        }

        async UniTask MakeTurn_Cards(int eventsCount)
        {
            int iterations = 0;
            CardsResultsListSet resultsSet = new();
            Start:
            if (!_side.Territory.DrawersAreNull)
                await UniTask.WhenAll(MakeTurn_Await(eventsCount), UniTask.Delay(TURN_DELAY_MS));

            if (iterations++ > ITERATIONS_MAX)
            {
                Debug.LogError("Too many iterations, cards skipped.");
                return;
            }
            float2 sidesWeight = GetSidesWeight(_side);
            IBattleWeightResult noPlaceResult = GetNoPlacementResult(_side, sidesWeight);
            IBattleSleeveCard[] sleeveCards = ((IEnumerable<IBattleSleeveCard>)_side.Sleeve).ToArray();
            IBattleSleeveCard[] availableCards = sleeveCards.Where(c => _side.CanAfford(c)).ToArray();
            TableConsole.LogToFile("ai", $"CARD PLACE ITERATION {iterations}");
            if (availableCards.Length == 0)
            {
                TableConsole.LogToFile("ai", $"CANNOT AFFORD ANY CARD (count: {sleeveCards.Length})");
                return;
            }

            await UniTask.SwitchToThreadPool();
            BeginWritingLastTerrLogs(this);

            for (int i = 0; i < sleeveCards.Length; i++)
            {
                IBattleSleeveCard card = sleeveCards[i];
                CardCurrency currency = card.Data.price.currency;
                if (!resultsSet.Contains(currency)) continue; // CardCurrency could be skipped

                IBattleWeightResult result;
                if (card.Data.isField)
                {
                    BattleFieldCardWeightResult fieldRes = GetBestFieldCardPlaceResult((BattleFieldCard)card, _side, sidesWeight);
                    if (fieldRes != null)
                         TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: abs: {fieldRes.WeightDeltaAbs}, rel: {fieldRes.WeightDeltaRel}, field: {fieldRes.Field.TableNameDebug}");
                    else TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: null (no fields or cannot afford)");
                    result = fieldRes;
                }
                else
                {
                    BattleFloatCardWeightResult floatRes = GetBestFloatCardUseResult((BattleFloatCard)card, _side, sidesWeight);
                    if (floatRes != null)
                         TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: abs: {floatRes.WeightDeltaAbs}, rel: {floatRes.WeightDeltaRel}");
                    else TableConsole.LogToFile("vrt", $"sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}, result: null (cannot afford)");
                    result = floatRes;
                }
                if (result != null && result.WeightDeltaAbs != 0 && result.WeightDeltaAbs > noPlaceResult.WeightDeltaAbs)
                    resultsSet[currency].Add(result);
            };

            StopWritingLastTerrLogs(this);
            await UniTask.SwitchToMainThread();

            for (int i = 0; i < resultsSet.Count; i++) // iterates for each CardCurrency in the game
            {
                CardsResultsList results = resultsSet[i];
                PickResultAgain:
                IBattleWeightResult result = GetBestResult(results);
                if (result != null)
                    results.Remove(result);
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
                if (!resultCard.TryDropOn(new TableSleeveCardDropArgs(resultField, false)))
                    Debug.LogError("Failed to drop a card on a field while making turn.");
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
        async UniTask MakeTurn_Traits(int eventsCount)
        {
            int iterations = 0;
            List<BattleActiveTraitWeightResult> lastResults = null;
            Start:
            if (!_side.Territory.DrawersAreNull)
                await UniTask.WhenAll(MakeTurn_Await(eventsCount), UniTask.Delay(TURN_DELAY_MS));

            if (iterations++ > ITERATIONS_MAX)
            {
                string lastResultsStr = string.Join(",\n", lastResults);
                Debug.LogWarning($"Too many iterations, traits skipped.\n{lastResultsStr}");
                return;
            }
            float2 sidesWeight = GetSidesWeight(_side);
            IBattleWeightResult noUseResult = GetNoPlacementResult(_side, sidesWeight);
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
            BeginWritingLastTerrLogs(this);

            List<BattleActiveTraitWeightResult> results = new();
            foreach (BattleActiveTraitListElement element in elements)
            {
                BattleActiveTraitWeightResult result = GetBestActiveTraitUseResult(element.Trait, _side, sidesWeight);
                IBattleThresholdUsable<BattleActiveTrait> usable = element.Trait.Data;
                BattleWeight threshold = result == null ? BattleWeight.Negative(element.Trait) : usable.WeightDeltaUseThreshold(result);

                if (result == null)
                    TableConsole.LogToFile("vrt", $"active trait: name: {element.Trait.TableNameDebug}, threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: null");
                else if (result.Field == null)
                     TableConsole.LogToFile("vrt", $"active trait: name: {element.Trait.TableNameDebug}, threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: abs: {result.WeightDeltaAbs}, rel: {result.WeightDeltaRel}, target: null");
                else TableConsole.LogToFile("vrt", $"active trait: name: {element.Trait.TableNameDebug}, threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: abs: {result.WeightDeltaAbs}, rel: {result.WeightDeltaRel}, target: {result.Field.TableNameDebug}");

                bool betterThanDoNothing = result != null && result.WeightDeltaAbs != 0 && result.WeightDeltaAbs > noUseResult.WeightDeltaAbs;
                if (betterThanDoNothing && usable.WeightIsEnough(result)) 
                    results.Add(result);
            }

            StopWritingLastTerrLogs(this);
            await UniTask.SwitchToMainThread();

            PickResultAgain:
            BattleActiveTraitWeightResult bestResult = GetBestResult(results);
            if (bestResult == null) return;
            results.Remove(bestResult);

            BattleActiveTrait activeTrait = bestResult.Entity;
            ActiveTrait activeTraitData = activeTrait.Data;
            TableActiveTraitUseArgs activeTraitUseArgs = new(activeTrait, bestResult.Field);
            if (!activeTraitData.IsUsable(activeTraitUseArgs) || !activeTraitData.Threshold.WeightIsEnough(bestResult))
                goto PickResultAgain;

            await UniTask.Delay(TURN_DELAY_MS);
            await activeTrait.TryUse(bestResult.Field);

            lastResults = results;
            goto Start;
        }
        UniTask MakeTurn_Await(int eventsCount)
        {
            if (!_side.Territory.DrawersAreNull)
                 return TableEventManager.AwaitAll(eventsCount);
            else return UniTask.CompletedTask;
        }

        static IBattleWeightResult GetNoPlacementResult(BattleSide aiSide, float2 sidesWeight)
        {
            BeginWritingLastTerrLogs(aiSide.ai);
            UseVirtual(null, aiSide, sidesWeight, out float2 deltas);
            StopWritingLastTerrLogsForNoPlacement(aiSide.ai);
            BattleFloatCardWeightResult result = new(null, deltas[0], deltas[1]);
            TableConsole.LogToFile("ai", $"NOP result: delta abs: {result.WeightDeltaAbs}.");
            return result;
        }
        static BattleFieldCardWeightResult GetBestFieldCardPlaceResult(BattleFieldCard card, BattleSide aiSide, float2 sidesWeight)
        {
            if (card.Side != aiSide)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            int2[] possibleFieldsPos = card.Side.Fields().WithoutCard().Select(f => f.pos).ToArray();
            BattleFieldCardWeightResult[] results = new BattleFieldCardWeightResult[possibleFieldsPos.Length];
            BattleTerritory srcTerritory = aiSide.Territory;

            for (int i = 0; i < possibleFieldsPos.Length; i++)
            {
                int2 fieldPos = possibleFieldsPos[i];
                BattleTerritory terrClone = CloneTerritory(aiSide.ai, aiSide.Territory, out BattleTerritoryCloneArgs terrCArgs);

                if (srcTerritory.GetType() != terrClone.GetType())
                    throw new Exception($"{srcTerritory.GetType()} must implement {nameof(ICloneableWithArgs)} in order to use BattleAI. " +
                                        $"Also consider implementing this interface in all classes used in territory.");

                BattleSide sideClone = aiSide.isMe ? terrClone.Player : terrClone.Enemy;
                FieldCard dataClone = (FieldCard)card.Data.Clone();
                BattleField fieldClone = terrClone.Field(fieldPos.x, fieldPos.y);
                BattleField cardFieldClone = card.Field == null ? null : terrClone.Field(card.Field.pos);
                BattleFieldCardCloneArgs cardCArgs = new(dataClone, cardFieldClone, sideClone, terrCArgs);
                BattleFieldCard cardClone = (BattleFieldCard)card.Clone(cardCArgs);

                try
                {
                    // will still run synchronously (handlers should check ITableObject.Drawer for null value)
                    _ = terrClone.PlaceFieldCard(cardClone, fieldClone, sideClone);
                    _ = terrClone.LastPhase();

                    BattleSide sideCloneOpposite = sideClone.Opposite;
                    float2 weightsWeightAfterTurn = new(sideClone.CalculateWeight(), sideCloneOpposite.CalculateWeight());
                    float2 weightDelta = CalculateWeightDelta(aiSide, sidesWeight, weightsWeightAfterTurn);
                    results[i] = new BattleFieldCardWeightResult(card, srcTerritory.Field(fieldPos), weightDelta[0], weightDelta[1]);
                }
                catch (Exception e) { Debug.LogException(e); }
                finally { terrClone.Dispose(); }
            }

            BattleFieldCardWeightResult bestResult = GetBestResult(results);
            return bestResult;
        }
        static BattleFloatCardWeightResult GetBestFloatCardUseResult(BattleFloatCard card, BattleSide aiSide, float2 sidesWeight)
        {
            if (card.Side != aiSide)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            // there's no point in cloning if object is not usable
            if (!card.IsUsable(new TableFloatCardUseArgs(card, card.Territory)))
                return null;

            void UseAction(BattleTerritory terr) => ((BattleFloatCard)card.Finder.FindInBattle(terr)).TryUse();
            if (!UseVirtual(UseAction, aiSide, sidesWeight, out float2 deltas))
                return null;

            BattleFloatCardWeightResult bestResult = new(card, deltas[0], deltas[1]); // single because cannot be placed on field (no difference)
            return bestResult;
        }
        static BattleActiveTraitWeightResult GetBestActiveTraitUseResult(BattleActiveTrait trait, BattleSide aiSide, float2 sidesWeight)
        {
            if (trait.Side != aiSide)
                throw new InvalidOperationException("Provided trait does not belong to this side.");

            List<BattleField> potTargets = new();
            List<BattleActiveTraitWeightResult> results = new();
            potTargets.AddRange(trait.Area.PotentialTargets());
            if (potTargets.Count == 0 && trait.Owner.Field != null) 
                potTargets.Add(trait.Owner.Field);

            foreach (BattleField target in potTargets)
            {
                // there's no point in cloning if object is not usable
                if (!trait.IsUsable(new TableActiveTraitUseArgs(trait, target)))
                    continue;
                void UseAction(BattleTerritory terr)
                {
                    BattleActiveTrait traitClone = trait.Finder.FindInBattle(terr) as BattleActiveTrait;
                    BattleField targetClone = target?.Finder.FindInBattle(terr) as BattleField;
                    _ = traitClone.TryUse(targetClone);
                }
                if (!UseVirtual(UseAction, aiSide, sidesWeight, out float2 deltas))
                    continue;

                BattleActiveTraitWeightResult result = new(trait, target, deltas[0], deltas[1]);
                results.Add(result);
            }
            BattleActiveTraitWeightResult bestResult = GetBestResult(results);
            return bestResult;
        }
        static T GetBestResult<T>(IReadOnlyCollection<T> results) where T : class, IBattleWeightResult
        {
            if (results.Count == 0)
                return null;

            List<T> positiveResults = new();
            List<T> negativeResults = new();
            foreach (T result in results)
            {
                if (result == null) continue;
                if (result.WeightDeltaAbs < 0)
                     negativeResults.Add(result);
                else positiveResults.Add(result);
            }

            if (positiveResults.Count + negativeResults.Count == 0)
                return null;

            List<T> targetList;
            T maxResult;
            if (positiveResults.Count != 0)
            {
                maxResult = positiveResults.Max();
                targetList = positiveResults;
            }
            else
            {
                maxResult = negativeResults.Max();
                targetList = negativeResults;
            }

            List<T> finalResults = new();
            foreach (T result in targetList)
            {
                if (result.Equals(maxResult))
                    finalResults.Add(result);
            }

            if (finalResults.Count != 0)
                return finalResults[Utils.RandomIntSafe(0, finalResults.Count)];
            else
            {
                Debug.LogWarning("AI final results are empty, but initial results are not.");
                return null;
            }
        }

        static bool UseVirtual(Action<BattleTerritory>? useFunc, BattleSide aiSide, float2 sidesWeight, out float2 deltas)
        {
            BattleTerritory terrClone = CloneTerritory(aiSide.ai, aiSide.Territory, out _);
            BattleSide sideClone = aiSide.isMe ? terrClone.Player : terrClone.Enemy;

            try
            {
                useFunc?.Invoke(terrClone);
                _ = terrClone.LastPhase();
            }
            catch (Exception e) 
            {
                Debug.LogException(e);
                deltas = float2.zero;
                terrClone.Dispose();
                return false; 
            }

            BattleSide sideCloneOpposite = sideClone.Opposite;
            float2 weightsWeightAfterTurn = new(sideClone.CalculateWeight(), sideCloneOpposite.CalculateWeight());

            deltas = CalculateWeightDelta(aiSide, sidesWeight, weightsWeightAfterTurn);
            terrClone.Dispose();
            return true;
        }
        static float2 CalculateWeightDelta(BattleSide aiSide, float2 sidesStartWeight, float2 sidesEndWeight)
        {
            // before/after turn
            float thisSideWeightBefore = ThisSideWeight_ScaledByStyle(aiSide.ai.Style, sidesStartWeight.x);     
            float thisSideWeightAfter  = ThisSideWeight_ScaledByStyle(aiSide.ai.Style, sidesEndWeight.x);

            float oppoSideWeightBefore = OppoSideWeight_ScaledByStyle(aiSide.ai.Style, sidesStartWeight.y);
            float oppoSideWeightAfter  = OppoSideWeight_ScaledByStyle(aiSide.ai.Style, sidesEndWeight.y);

            // if sidesStartWeight sum is 200 and weightDeltaAbs is 40,
            // weightDeltaRel equals to (40 / 200) = 0.2
            float weightDeltaAbs = (thisSideWeightAfter - thisSideWeightBefore) - (oppoSideWeightAfter - oppoSideWeightBefore);
            float weightDeltaRel = weightDeltaAbs / (thisSideWeightBefore + oppoSideWeightBefore);

            return new float2(weightDeltaAbs, weightDeltaRel);
        }

        static float ThisSideWeight_ScaledByStyle(PlayStyle style, float weight)
        {
            if (style == PlayStyle.Defensive)
                 return weight * 2;
            else return weight;
        }
        static float OppoSideWeight_ScaledByStyle(PlayStyle style, float weight)
        {
            if (style == PlayStyle.Offensive)
                 return weight * 2;
            else return weight;
        }

        static void BeginWritingLastTerrLogs(BattleAI ai)
        {
            TableConsole.OnLogToFile += ai.OnConsoleLogToFile;
        }
        static void StopWritingLastTerrLogs(BattleAI ai)
        {
            TableConsole.OnLogToFile -= ai.OnConsoleLogToFile;
            TableConsole.LogToFile("ai", $"LAST AI TERRITORY CLONE LOGS START");
            TableConsole.LogToFile("ai", ai._lastTerrCloneLogs);
            TableConsole.LogToFile("ai", $"LAST AI TERRITORY CLONE LOGS END");
            ai._lastTerrCloneLogs.Clear();
        }
        static void StopWritingLastTerrLogsForNoPlacement(BattleAI ai)
        {
            TableConsole.OnLogToFile -= ai.OnConsoleLogToFile;
            TableConsole.LogToFile("ai", $"NOP AI TERRITORY CLONE LOGS START");
            TableConsole.LogToFile("ai", ai._lastTerrCloneLogs);
            TableConsole.LogToFile("ai", $"NOP AI TERRITORY CLONE LOGS END");
            ai._lastTerrCloneLogs.Clear();
        }

        static BattleTerritory CloneTerritory(BattleAI ai, BattleTerritory src, out BattleTerritoryCloneArgs cArgs)
        {
            ai._lastTerrCloneLogs.Clear();
            cArgs = new BattleTerritoryCloneArgs();
            return (BattleTerritory)src.Clone(cArgs);
        }
        static float2 GetSidesWeight(BattleSide aiSide, params int[] excludedWeights)
        {
            return new(aiSide.CalculateWeight(excludedWeights), aiSide.Opposite.CalculateWeight(excludedWeights));
        }
        bool OnConsoleLogToFile(string text)
        {
            if (text.StartsWith(nameof(BattleAI)))
                return false;
            _lastTerrCloneLogs.Add(text);
            return true;
        }
    }
}
