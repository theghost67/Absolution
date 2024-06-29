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
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий ИИ-действия стороны сражения (см. <see cref="BattleSide"/>).
    /// </summary>
    public sealed class BattleAI
    {
        const int TURN_DELAY = 500;
        const int MAX_ITERATIONS = 30;

        const string TERMINATOR1 = "********";
        const string TERMINATOR8 = "****************************************************************";

        readonly BattleSide _side;

        public PlayStyle Style
        {
            get => _style;
            set
            {
                if (_isMakingTurn)
                    throw new InvalidOperationException($"Cannot change {nameof(BattleAI)} properties when AI {nameof(IsMakingTurn)}.");
                _style = value;
            }
        }
        public bool UsesAiming
        {
            get => _usesAiming;
            set
            {
                if (_isMakingTurn)
                    throw new InvalidOperationException($"Cannot change {nameof(BattleAI)} properties when AI {nameof(IsMakingTurn)}.");
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
                Debug.LogError($"{nameof(BattleAI)}: can't make a turn because it's not AI's place phase or already making turn.");
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
                TableConsole.LogToFile($"{TERMINATOR8}\n{nameof(BattleAI)}: TURN STARTED\n{TERMINATOR8}");
                await MakeTurn_Cards(sidesWeight, GetNoPlacementResult(sidesWeight));
                await MakeTurn_Traits(sidesWeight, GetNoPlacementResult(sidesWeight));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _lastTerr?.Dispose();
            }
            #endif

            TableConsole.LogToFile($"{TERMINATOR8}\n{nameof(BattleAI)}: TURN ENDED\n{TERMINATOR8}");
            _turnTween.Kill();
            _isMakingTurn = false;
            _side.Territory.NextPhase();
        }

        // TODO[IMPORTANT]: await async queue before making turn (cards/traits)
        async UniTask MakeTurn_Cards(float2 sidesWeight, IBattleWeightResult noPlaceResult)
        {
            int iterations = 0;
            IBattleSleeveCard[] sleeveCards = ((IEnumerable<IBattleSleeveCard>)_side.Sleeve).ToArray();
            CardsResultsListSet resultsSet = new();

            PlaceAnotherCard:
            IBattleSleeveCard[] availableCards = sleeveCards.Where(c => _side.CanAfford(c)).ToArray();
            if (availableCards.Length == 0)
            {
                TableConsole.LogToFile($"{TERMINATOR1} {nameof(BattleAI)}: CANNOT AFFORD ANY CARD {TERMINATOR1}");
                return;
            }
            if (!_isMakingTurn || iterations++ > MAX_ITERATIONS)
            {
                Debug.LogError($"{nameof(BattleAI)}: card turn interrupted (too many iterations or timed out).");
                return;
            }

            await UniTask.SwitchToThreadPool();
            BeginWriteLastTerrLogs();

            for (int i = 0; i < sleeveCards.Length; i++)
            {
                IBattleSleeveCard card = sleeveCards[i];
                CardCurrency currency = card.Data.price.currency;
                if (!resultsSet.Contains(currency)) continue; // CardCurrency could be skipped

                IBattleWeightResult result;
                if (card.Data.isField)
                    result = GetBestFieldCardPlaceResult((BattleFieldCard)card, sidesWeight);
                else result = GetBestFloatCardUseResult((BattleFloatCard)card, sidesWeight);

                if (result != null && result.WeightDeltaAbs > noPlaceResult.WeightDeltaAbs)
                    resultsSet[currency].Add(result);
            };

            StopWriteLastTerrLogs(forCards: true);
            await UniTask.SwitchToMainThread();

            for (int i = 0; i < resultsSet.Count; i++) // iterates for each CardCurrency in the game
            {
                CardsResultsList results = resultsSet[i];

                PickResultAgain:
                if (results.Count == 0) continue;
                int index = results.GetWeightedRandomIndex(r => Mathf.Pow(r.WeightDeltaAbs, 2));
                if (index == -1) continue; 
                IBattleWeightResult result = results[index];
                results.RemoveAt(index);

                IBattleSleeveCard resultCard = (IBattleSleeveCard)result.Entity;
                if (!_side.CanAfford(resultCard))
                {
                    resultsSet.Remove(results); // skips all cards with the same price type (waits for next turn)
                    i--; continue;
                }

                TableConsole.LogToFile($"{TERMINATOR1} {nameof(BattleAI)}: CARD PLACEMENT BEGINS: {result.Entity.TableNameDebug} {TERMINATOR1}");
                if (result is BattleFieldCardWeightResult resultOfFieldCard)
                {
                    resultCard.TryDropOn(resultOfFieldCard.field);
                    await UniTask.Delay(TURN_DELAY);
                    continue;
                }

                BattleFloatCardWeightResult resultOfFloatCard = (BattleFloatCardWeightResult)result;
                BattleFloatCard floatCard = resultOfFloatCard.Entity;
                FloatCard floatCardData = floatCard.Data;
                TableFloatCardUseArgs floatCardUseArgs = new(floatCard, _side.Territory);

                if (!floatCardData.IsUsable(floatCardUseArgs) || !floatCardData.Threshold.WeightIsEnough(resultOfFloatCard))
                    goto PickResultAgain;

                resultCard.TryDropOn(null);
                await TableEventManager.WhenAll();
                await UniTask.Delay(TURN_DELAY);

                if (resultsSet.Count != 0)
                    goto PlaceAnotherCard;
            }
        }
        async UniTask MakeTurn_Traits(float2 sidesWeight, IBattleWeightResult noUseResult)
        {
            int iterations = 0;

            UseAnotherTrait:
            if (!_isMakingTurn || iterations++ > MAX_ITERATIONS)
            {
                Debug.LogError($"{nameof(BattleAI)}: trait turn interrupted (too many iterations or timed out).");
                return;
            }

            List<BattleActiveTraitListElement> elements = new();
            foreach (BattleField field in _side.Fields().WithCard())
            {
                foreach (BattleActiveTraitListElement element in field.Card.Traits.Actives)
                    elements.Add(element);
            }

            await UniTask.SwitchToThreadPool();
            BeginWriteLastTerrLogs();

            List<BattleActiveTraitWeightResult> results = new();
            foreach (BattleActiveTraitListElement element in elements)
            {
                BattleActiveTraitWeightResult result = GetBestActiveTraitUseResult(element.Trait, sidesWeight);
                if (result != null && result.WeightDeltaAbs > noUseResult.WeightDeltaAbs) 
                    results.Add(result);
            }

            StopWriteLastTerrLogs(forCards: false);
            await UniTask.SwitchToMainThread();

            PickResultAgain:
            if (results.Count == 0)
            {
                TableConsole.LogToFile($"{TERMINATOR1} {nameof(BattleAI)}: NO TRAITS FOUND {TERMINATOR1}");
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
            static bool UseFunc(BattleTerritory terr)
            {
                terr.LastPhase();
                return true;
            }

            BeginWriteLastTerrLogs();
            VirtualUseIsPossible(UseFunc, sidesWeight, out float2 deltas);
            StopWriteLastTerrLogsForNoPlacement();
            BattleFloatCardWeightResult result = new(null, deltas[0], deltas[1]);
            TableConsole.LogToFile($"{nameof(BattleAI)}: >> NOP result: delta abs: {result.WeightDeltaAbs}.");
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
                    throw new Exception($"{srcTerritory.GetType()} must implement {nameof(ICloneableWithArgs)} in order to use {nameof(BattleAI)}. " +
                                        $"Also consider implementing this interface in all classes used in territory.");

                BattleSide sideClone = _side.isMe ? terrClone.player : terrClone.enemy;
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
                if (Config.writeAllAiResultsLogs)
                    TableConsole.LogToFile($"{nameof(BattleAI)}: >> field card result: name: {card.TableNameDebug}, delta abs: {results[p].WeightDeltaAbs}, field: {results[p].field.TableNameDebug}, can afford: {_side.CanAfford(card)}.");
            }

            BattleFieldCardWeightResult bestResult = results.GetWeightedRandom(r => ResultRandomPicker(r));
            TableConsole.LogToFile($"{nameof(BattleAI)}: >> BEST: field card result: name: {card.TableNameDebug}, delta abs: {bestResult.WeightDeltaAbs}, field: {bestResult.field.TableNameDebug}, can afford: {_side.CanAfford(card)}.");
            return bestResult;
        }
        BattleFloatCardWeightResult GetBestFloatCardUseResult(BattleFloatCard card, float2 sidesWeight)
        {
            if (card.Side != _side)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            bool UseFunc(BattleTerritory terr) => ((BattleFloatCard)card.Finder.FindInBattle(terr)).TryUse();
            bool possibleToUse = VirtualUseIsPossible(UseFunc, sidesWeight, out float2 deltas);

            if (!possibleToUse)
                return null;

            BattleFloatCardWeightResult bestResult = new(card, deltas[0], deltas[1]); // single because cannot be placed on field (no difference)
            TableConsole.LogToFile($"{nameof(BattleAI)}: >> BEST: float card result: name: {card.TableNameDebug}, delta abs: {bestResult.WeightDeltaAbs}, can afford: {_side.CanAfford(card)}.");
            return bestResult;
        }
        BattleActiveTraitWeightResult GetBestActiveTraitUseResult(BattleActiveTrait trait, float2 sidesWeight)
        {
            if (trait.Side != _side)
                throw new InvalidOperationException("Provided trait does not belong to this side.");

            BattleField[] potTargets = trait.Area.PotentialTargets().ToArray();
            List<BattleActiveTraitWeightResult> results = new(potTargets.Length);
            foreach (BattleField target in potTargets)
            {
                bool UseFunc(BattleTerritory terr) => ((BattleActiveTrait)trait.Finder.FindInBattle(terr)).TryUse(target);
                bool possibleToUse = VirtualUseIsPossible(UseFunc, sidesWeight, out float2 deltas);
                if (!possibleToUse) continue;
                BattleActiveTraitWeightResult result = new(trait, target, deltas[0], deltas[1]);
                results.Add(result);
                if (Config.writeAllAiResultsLogs)
                    TableConsole.LogToFile($"{nameof(BattleAI)}: >> active trait result: name: {trait.TableNameDebug}, delta: {result.WeightDeltaAbs}, target: {result.target.TableNameDebug}.");
            }

            if (results.Count == 0)
                return null;

            BattleActiveTraitWeightResult bestResult = results.GetWeightedRandom(r => ResultRandomPicker(r));
            TableConsole.LogToFile($"{nameof(BattleAI)}: >> BEST: active trait result: name: {trait.TableNameDebug}, delta: {bestResult.WeightDeltaAbs}, target: {bestResult.target.TableNameDebug}.");
            return bestResult;
        }
        static float ResultRandomPicker(IBattleWeightResult result)
        {
            if (result.WeightDeltaAbs > 0)
                 return Mathf.Pow(result.WeightDeltaAbs, 3);
            else return 0.000001f; // can still choose results with negative value (will use it if it's better than do nothing)
        }

        bool VirtualUseIsPossible(Func<BattleTerritory, bool> useFunc, float2 sidesWeight, out float2 deltas)
        {
            BattleTerritory terrClone = CloneTerritory(_side.Territory, out _);
            BattleSide sideClone = _side.isMe ? terrClone.player : terrClone.enemy;

            if (!useFunc(terrClone))
            {
                deltas = 0;
                return false;
            }

            deltas = CalculateWeightDeltas(sidesWeight, sideClone, sideClone.Opposite);
            terrClone.Dispose();
            return true;
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
            TableConsole.LogToFile($"{TERMINATOR1} LAST AI TERRITORY CLONE LOGS START ({(forCards ? "CARDS" : "TRAITS")}) {TERMINATOR1}");
            TableConsole.LogToFile(_lastTerrLogs);
            TableConsole.LogToFile($"{TERMINATOR1} LAST AI TERRITORY CLONE LOGS END ({(forCards ? "CARDS" : "TRAITS")}) {TERMINATOR1}");
            _lastTerrLogs.Clear();
        }
        void StopWriteLastTerrLogsForNoPlacement()
        {
            TableConsole.OnLogToFile -= OnConsoleLogToFile;
            TableConsole.LogToFile($"{TERMINATOR1} NOP AI TERRITORY CLONE LOGS START {TERMINATOR1}");
            TableConsole.LogToFile(_lastTerrLogs);
            TableConsole.LogToFile($"{TERMINATOR1} NOP AI TERRITORY CLONE LOGS END {TERMINATOR1}");
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
