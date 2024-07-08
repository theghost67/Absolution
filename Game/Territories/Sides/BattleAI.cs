//#define LEGACY_AI

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
        const int TURN_DELAY = 500;
        const int MAX_ITERATIONS = 16;

        const string TERMINATOR1 = "********";
        const string TERMINATOR8 = "****************************************************************";

        readonly BattleSide _side;

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
        public bool IsMakingTurn => _isMakingTurn;

        List<string> _lastTerrLogs;
        BattleTerritory _lastTerr;
        Tween _turnTween;

        PlayStyle _style;
        bool _usesAiming;
        bool _isMakingTurn;

        public enum PlayStyle
        {
            Balanced,
            Defensive,
            Offensive,
        }
        class CardsResultsListSet : IEnumerable<CardsResultsList>
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

            IEnumerator<CardsResultsList> IEnumerable<CardsResultsList>.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }
        class CardsResultsList : IEnumerable<IBattleWeightResult>, IEquatable<CardsResultsList>
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
            await TableEventManager.WhenAll();
            if (_isMakingTurn || _side.Territory.PhaseSide != _side)
            {
                Debug.LogError($"BattleAI: can't make a turn because it's not AI's place phase or already making turn.");
                return;
            }

            _isMakingTurn = true;
            _turnTween = DOVirtual.DelayedCall(5, () =>
            {
                _isMakingTurn = false;
                _lastTerr?.Dispose();
            });

            float2 sidesWeight = new(_side.Weight, _side.Opposite.Weight);
            await UniTask.Delay(TURN_DELAY);

            #if LEGACY_AI
            await MakeTurn_Legacy();
            #else
            try
            {
                TableConsole.LogToFile($"{TERMINATOR8}\nBattleAI: TURN STARTED\n{TERMINATOR8}");
                await MakeTurn_Cards(sidesWeight, GetNoPlacementResult(sidesWeight));
                await MakeTurn_Traits(sidesWeight, GetNoPlacementResult(sidesWeight));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _lastTerr?.Dispose();
            }
            #endif

            TableConsole.LogToFile($"{TERMINATOR8}\nBattleAI: TURN ENDED\n{TERMINATOR8}");
            _turnTween.Kill();
            _isMakingTurn = false;
            _side.Territory.NextPhase();
        }

        // TODO[IMPORTANT]: await async queue before making turn (cards/traits)
        async UniTask MakeTurn_Cards(float2 sidesWeight, IBattleWeightResult noPlaceResult)
        {
            int iterations = 0;
            CardsResultsListSet resultsSet = new();

            Start:
            IBattleSleeveCard[] sleeveCards = ((IEnumerable<IBattleSleeveCard>)_side.Sleeve).ToArray();
            IBattleSleeveCard[] availableCards = sleeveCards.Where(c => _side.CanAfford(c)).ToArray();
            TableConsole.LogToFile($"{TERMINATOR1} BattleAI: ITERATION {iterations} {TERMINATOR1}");

            if (availableCards.Length == 0)
            {
                TableConsole.LogToFile($"{TERMINATOR1} BattleAI: CANNOT AFFORD ANY CARD {TERMINATOR1}");
                return;
            }
            if (!_isMakingTurn || iterations++ > MAX_ITERATIONS)
            {
                Debug.LogError($"BattleAI: card turn interrupted (too many iterations or timed out).");
                return;
            }

            _turnTween.timeScale = 0;
            await UniTask.SwitchToThreadPool();
            BeginWriteLastTerrLogs();

            for (int i = 0; i < sleeveCards.Length; i++)
            {
                IBattleSleeveCard card = sleeveCards[i];
                CardCurrency currency = card.Data.price.currency;
                if (!resultsSet.Contains(currency)) continue; // CardCurrency could be skipped

                IBattleWeightResult result;
                if (card.Data.isField)
                {
                    BattleFieldCardWeightResult fieldRes = GetBestFieldCardPlaceResult((BattleFieldCard)card, sidesWeight);
                    TableConsole.LogToFile($"BattleAI: >> sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}\n" +
                                           $"             result: abs: {fieldRes.WeightDeltaAbs}, rel: {fieldRes.WeightDeltaRel}, field: {fieldRes.field.TableNameDebug}");
                    result = fieldRes;
                }
                else
                {
                    BattleFloatCardWeightResult floatRes = GetBestFloatCardUseResult((BattleFloatCard)card, sidesWeight);
                    TableConsole.LogToFile($"BattleAI: >> sleeve field card: name: {card.TableNameDebug}, can afford: {_side.CanAfford(card)}\n" +
                                           $"             result: abs: {floatRes.WeightDeltaAbs}, rel: {floatRes.WeightDeltaRel}");
                    result = floatRes;
                }
                if (result != null && result.WeightDeltaAbs > noPlaceResult.WeightDeltaAbs)
                    resultsSet[currency].Add(result);
            };

            StopWriteLastTerrLogs(forCards: true);
            await UniTask.SwitchToMainThread();
            _turnTween.timeScale = 1;

            int resultsCount = 0;
            for (int i = 0; i < resultsSet.Count; i++) // iterates for each CardCurrency in the game
            {
                CardsResultsList results = resultsSet[i];
                resultsCount += results.Count;

                PickResultAgain:
                if (results.Count == 0) continue;
                int index = results.GetWeightedRandomIndex(r => Mathf.Pow(r.WeightDeltaAbs, 2));
                if (index == -1) continue; 
                IBattleWeightResult result = results[index];
                results.RemoveAt(index);

                IBattleSleeveCard resultCard = (IBattleSleeveCard)result.Entity;
                BattleField resultField = null;

                if (!_side.CanAfford(resultCard))
                {
                    resultsSet.Remove(results); // skips all cards with the same price type (waits for next turn)
                    i--; continue;
                }

                TableConsole.LogToFile($"{TERMINATOR1} BattleAI: CARD PLACEMENT BEGINS: {result.Entity.TableNameDebug} {TERMINATOR1}");
                if (result is BattleFieldCardWeightResult resultOfFieldCard)
                    resultField = resultOfFieldCard.field;
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
                await TableEventManager.WhenAll();
                await UniTask.Delay(TURN_DELAY);
                TableConsole.LogToFile($"{TERMINATOR1} BattleAI: CARD PLACEMENT ENDS: {result.Entity.TableNameDebug} {TERMINATOR1}");
                goto End;
            }

            End:
            if (resultsCount != 0)
                goto Start;
        }
        async UniTask MakeTurn_Traits(float2 sidesWeight, IBattleWeightResult noUseResult)
        {
            int iterations = 0;

            UseAnotherTrait:
            if (!_isMakingTurn || iterations++ > MAX_ITERATIONS)
            {
                Debug.LogError($"BattleAI: trait turn interrupted (too many iterations or timed out).");
                return;
            }

            // adds active traits from sleeve cards and cards placed on side's fields to list
            List<BattleActiveTraitListElement> elements = new();
            IEnumerable<IBattleSleeveCard> sleeveCards = _side.Sleeve;
            IEnumerable<BattleFieldCard> cards = _side.Fields().WithCard().Select(f => f.Card).Concat(sleeveCards.Where(c => c is BattleFieldCard).Cast<BattleFieldCard>());
            foreach (BattleActiveTraitListElement element in cards.SelectMany<BattleFieldCard, BattleActiveTraitListElement>(f => f.Traits.Actives))
                elements.Add(element);

            _turnTween.timeScale = 0;
            await UniTask.SwitchToThreadPool();
            BeginWriteLastTerrLogs();

            List<BattleActiveTraitWeightResult> results = new();
            foreach (BattleActiveTraitListElement element in elements)
            {
                BattleWeight threshold = element.Trait.Data.WeightDeltaUseThreshold(element.Trait);
                BattleActiveTraitWeightResult result = GetBestActiveTraitUseResult(element.Trait, sidesWeight);

                if (result != null)
                     TableConsole.LogToFile($"BattleAI: >> active trait: name: {element.Trait.TableNameDebug}\n" + 
                                            $"             threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: abs: {result.WeightDeltaAbs}, rel: {result.WeightDeltaRel}");
                else TableConsole.LogToFile($"BattleAI: >> active trait: name: {element.Trait.TableNameDebug}\n" +
                                            $"             threshold: abs: {threshold.absolute}, rel: {threshold.relative}; result: null (impossible to use)");

                if (result != null && result.WeightDeltaAbs > noUseResult.WeightDeltaAbs) 
                    results.Add(result);
            }

            StopWriteLastTerrLogs(forCards: false);
            await UniTask.SwitchToMainThread();
            _turnTween.timeScale = 1;

            PickResultAgain:
            if (results.Count == 0)
            {
                TableConsole.LogToFile($"{TERMINATOR1} BattleAI: NO TRAITS FOUND {TERMINATOR1}");
                return;
            }

            int resultIndex = results.GetWeightedRandomIndex(r => Mathf.Pow(r.WeightDeltaAbs, 2));
            BattleActiveTraitWeightResult bestResult = results[resultIndex];
            results.RemoveAt(resultIndex);

            BattleActiveTrait activeTrait = bestResult.Entity;
            ActiveTrait activeTraitData = activeTrait.Data;
            TableActiveTraitUseArgs activeTraitUseArgs = new(activeTrait, bestResult.target);

            if (!activeTraitData.IsUsable(activeTraitUseArgs) || !activeTraitData.Threshold.WeightIsEnough(bestResult))
                goto PickResultAgain;

            activeTrait.TryUse(bestResult.target);
            await TableEventManager.WhenAll();
            await UniTask.Delay(TURN_DELAY);

            goto UseAnotherTrait;
        }

        #if LEGACY_AI
        async UniTask MakeTurn_Legacy()
        {
            BattleField[] oppositeFieldsWithCard = _side.Opposite.Fields().WithCard().ToArray();
            if (oppositeFieldsWithCard.Length == 0) goto SkipDefend;
            int defendAttempts = 0;
            while (defendAttempts < BattleTerritory.MAX_WIDTH) // defend
            {
                BattleField oppositeFieldWithCard = oppositeFieldsWithCard.GetRandom();
                BattleField myField = _side.Territory.FieldOpposite(oppositeFieldWithCard.pos);
                if (myField.Card != null)
                {
                    defendAttempts++;
                    continue;
                }

                IBattleSleeveCard[] availableCards = ((IEnumerable<IBattleSleeveCard>)_side.Sleeve).Where(c => c.Data.isField && _side.CanAfford(c)).ToArray();
                if (availableCards.Length == 0) return;

                defendAttempts++;
                availableCards.GetRandom().TryDropOn(myField);
                await TableEventManager.WhenAll();
            }

            SkipDefend:
            BattleField[] oppositeFieldsWithoutCard = _side.Opposite.Fields().WithoutCard().ToArray();
            if (oppositeFieldsWithoutCard.Length == 0) goto SkipDefend;
            int attackAttempts = 0;
            while (attackAttempts < BattleTerritory.MAX_WIDTH) // attack
            {
                BattleField oppositeFieldWithoutCard = oppositeFieldsWithoutCard.GetRandom();
                BattleField myField = _side.Territory.FieldOpposite(oppositeFieldWithoutCard.pos);
                if (myField.Card != null)
                {
                    attackAttempts++;
                    continue;
                }

                IBattleSleeveCard[] availableCards = ((IEnumerable<IBattleSleeveCard>)_side.Sleeve).Where(c => c.Data.isField && _side.CanAfford(c)).ToArray();
                if (availableCards.Length == 0) return;

                attackAttempts++;
                availableCards.GetRandom().TryDropOn(myField);
                await TableEventManager.WhenAll();
            }
        }
        #endif

        IBattleWeightResult GetNoPlacementResult(float2 sidesWeight)
        {
            BeginWriteLastTerrLogs();
            UseVirtual(terr => terr.LastPhase(), sidesWeight, out float2 deltas);
            StopWriteLastTerrLogsForNoPlacement();
            BattleFloatCardWeightResult result = new(null, deltas[0], deltas[1]);
            TableConsole.LogToFile($"BattleAI: >> NOP result: delta abs: {result.WeightDeltaAbs}.");
            return result;
        }
        BattleFieldCardWeightResult GetBestFieldCardPlaceResult(BattleFieldCard card, float2 sidesWeight)
        {
            if (card.Side != _side)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            int2[] possibleFieldsPos = card.Side.Fields().WithoutCard().Select(f => f.pos).ToArray(); // TODO[in next updates]: check if card trait has Attr.ALLOWS_TO_STACK_CARDS
            BattleFieldCardWeightResult[] results = new BattleFieldCardWeightResult[possibleFieldsPos.Length];
            BattleTerritory srcTerritory = _side.Territory;

            for (int p = 0; p < possibleFieldsPos.Length; p++)
            {
                int2 fieldPos = possibleFieldsPos[p];
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

                float2 deltas = CalculateWeightDeltas(sidesWeight, sideClone, sideClone.Opposite);
                results[p] = new BattleFieldCardWeightResult(card, srcTerritory.Field(fieldPos), deltas[0], deltas[1]);

                terrClone.Dispose();
            }

            BattleFieldCardWeightResult bestResult = GetBestResult(results);
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

                void UseAction(BattleTerritory terr) => ((BattleActiveTrait)trait.Finder.FindInBattle(terr)).TryUse(target);
                UseVirtual(UseAction, sidesWeight, out float2 deltas);

                BattleActiveTraitWeightResult result = new(trait, target, deltas[0], deltas[1]);
                results.Add(result);
            }
            BattleActiveTraitWeightResult bestResult = GetBestResult(results);
            return bestResult;
        }
        static T GetBestResult<T>(IReadOnlyCollection<T> results) where T : IBattleWeightResult
        {
            if (results.Count == 0)
                return default;

            List<T> positiveResults = new();
            List<T> negativeResults = new();
            foreach (T result in results)
            {
                if (result.WeightDeltaAbs < 0)
                     negativeResults.Add(result);
                else positiveResults.Add(result);
            }
            if (positiveResults.Count != 0)
                return positiveResults.GetWeightedRandom(r => Mathf.Pow(r.WeightDeltaAbs, 3));
            else if (negativeResults.Count != 0)
                return negativeResults.GetWeightedRandom(r => Mathf.Pow(-r.WeightDeltaAbs, 3));
            else return default;
        }

        void UseVirtual(Action<BattleTerritory> useFunc, float2 sidesWeight, out float2 deltas)
        {
            BattleTerritory terrClone = CloneTerritory(_side.Territory, out _);
            BattleSide sideClone = _side.isMe ? terrClone.Player : terrClone.Enemy;

            useFunc(terrClone);
            deltas = CalculateWeightDeltas(sidesWeight, sideClone, sideClone.Opposite);
            terrClone.Dispose();
        }
        float2 CalculateWeightDeltas(float2 sidesStartWeight, BattleSide thisSideAfterTurn, BattleSide oppoSideAfterTurn)
        {
            // before/after turn
            float thisSideWeightBefore = ThisSideWeight_ScaledByStyle(_side, sidesStartWeight.x);     
            float thisSideWeightAfter  = ThisSideWeight_ScaledByStyle(thisSideAfterTurn, thisSideAfterTurn.Weight);

            float oppoSideWeightBefore = OppoSideWeight_ScaledByStyle(_side.Opposite, sidesStartWeight.y);
            float oppoSideWeightAfter  = OppoSideWeight_ScaledByStyle(thisSideAfterTurn, oppoSideAfterTurn.Weight);

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

        void BeginWriteLastTerrLogs()
        {
            TableConsole.OnLogToFile += OnConsoleLogToFile;
        }
        void StopWriteLastTerrLogs(bool forCards)
        {
            TableConsole.OnLogToFile -= OnConsoleLogToFile;
            TableConsole.LogToFile($"{TERMINATOR1} BattleAI: LAST AI TERRITORY CLONE LOGS START ({(forCards ? "CARDS" : "TRAITS")}) {TERMINATOR1}");
            TableConsole.LogToFile(_lastTerrLogs);
            TableConsole.LogToFile($"{TERMINATOR1} BattleAI: LAST AI TERRITORY CLONE LOGS END ({(forCards ? "CARDS" : "TRAITS")}) {TERMINATOR1}");
            _lastTerrLogs.Clear();
        }
        void StopWriteLastTerrLogsForNoPlacement()
        {
            TableConsole.OnLogToFile -= OnConsoleLogToFile;
            TableConsole.LogToFile($"{TERMINATOR1} BattleAI: NOP AI TERRITORY CLONE LOGS START {TERMINATOR1}");
            TableConsole.LogToFile(_lastTerrLogs);
            TableConsole.LogToFile($"{TERMINATOR1} BattleAI: NOP AI TERRITORY CLONE LOGS END {TERMINATOR1}");
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
