#define LEGACY_AI

using Cysharp.Threading.Tasks;
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
        const int MAX_ITERATIONS = 30;

        static BattleTerritory _lastTerritory;
        readonly BattleSide _side;

        public static string LastTerritoryLog => _lastTerritory?.Log;
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

            float2 sidesWeight = new(_side.Weight, _side.Opposite.Weight);
            _isMakingTurn = true;

            await UniTask.Delay(TURN_DELAY);
            #if LEGACY_AI
            await MakeTurn_Legacy();
            #else
            await MakeTurn_Cards(sidesWeight, GetNoPlacementResult(sidesWeight));
            await MakeTurn_Traits(sidesWeight, GetNoPlacementResult(sidesWeight));
            #endif

            _isMakingTurn = false;
            _side.Territory.NextPhase();
        }

        // TODO[IMPORTANT]: await async queue before making turn (cards/traits)
        async UniTask MakeTurn_Cards(float2 sidesWeight, IBattleWeightResult noPlaceResult)
        {
            int iterations = 0;
            IEnumerable<IBattleSleeveCard> sleeveCards = _side.Sleeve;
            CardsResultsListSet resultsSet = new();

            PlaceAnotherCard:
            IBattleSleeveCard[] availableCards = sleeveCards.Where(c => _side.CanAfford(c)).ToArray();
            if (availableCards.Length == 0) return; // cannot afford any card
            if (iterations++ > MAX_ITERATIONS)
            {
                Debug.LogError($"{nameof(BattleAI)}: card turn interrupted (too many iterations).");
                return;
            }
            
            foreach (IBattleSleeveCard card in sleeveCards)
            {
                CardCurrency currency = card.Data.price.currency;
                if (!resultsSet.Contains(currency)) continue; // CardCurrency could be skipped

                IBattleWeightResult result;
                if (card.Data.isField)
                     result = GetBestFieldCardPlaceResult((BattleFieldCard)card, sidesWeight);
                else result = GetBestFloatCardUseResult((BattleFloatCard)card, sidesWeight);

                if (result != null && result.WeightDeltaAbs > noPlaceResult.WeightDeltaAbs)
                    resultsSet[currency].Add(result);
            }

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
            if (iterations++ > MAX_ITERATIONS)
            {
                Debug.LogError($"{nameof(BattleAI)}: trait turn interrupted (too many iterations).");
                return;
            }

            List<BattleActiveTraitListElement> elements = new();
            foreach (BattleField field in _side.Fields().WithCard())
            {
                foreach (BattleActiveTraitListElement element in field.Card.Traits.Actives)
                    elements.Add(element);
            }

            List<BattleActiveTraitWeightResult> results = new();
            foreach (BattleActiveTraitListElement element in elements)
            {
                BattleActiveTraitWeightResult result = GetBestActiveTraitUseResult(element.Trait, sidesWeight);
                if (result != null && result.WeightDeltaAbs > noUseResult.WeightDeltaAbs) 
                    results.Add(result);
            }

            PickResultAgain:
            if (results.Count == 0) return;
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

            VirtualUseIsPossible(UseFunc, sidesWeight, out float2 deltas);
            return new BattleFloatCardWeightResult(null, deltas[0], deltas[1]);
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
            }

            return results.Max();
        }
        BattleFloatCardWeightResult GetBestFloatCardUseResult(BattleFloatCard card, float2 sidesWeight)
        {
            if (card.Side != _side)
                throw new InvalidOperationException("Provided card does not belong to this side.");

            bool UseFunc(BattleTerritory terr) => ((BattleFloatCard)card.Finder.FindInBattle(terr)).TryUse();
            bool possibleToUse = VirtualUseIsPossible(UseFunc, sidesWeight, out float2 deltas);

            if (possibleToUse)
                return new BattleFloatCardWeightResult(card, deltas[0], deltas[1]);
            else return null;
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
                if (possibleToUse)
                    results.Add(new BattleActiveTraitWeightResult(trait, target, deltas[0], deltas[1]));
            }

            if (results.Count != 0)
                return results.Max();
            else return null;
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

        static BattleTerritory CloneTerritory(BattleTerritory src, out BattleTerritoryCloneArgs cArgs)
        {
            cArgs = new BattleTerritoryCloneArgs();
            _lastTerritory = (BattleTerritory)src.Clone(cArgs);
            return _lastTerritory;
        }
    }
}
