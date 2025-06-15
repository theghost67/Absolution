using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Sleeves;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, содержащий и отображающий данные об одной из сторон, находящейся на территории сражения.
    /// </summary>
    public class BattleSide : TableObject, IBattleKillable, ITableEntrySource, ICloneableWithArgs, IEquatable<BattleSide>
    {
        public BattleTerritory Territory => _territory;
        public BattleSide Opposite => _territory.GetOppositeOf(this);
        public TableFinder Finder => _finder;

        public new BattleSideDrawer Drawer => ((TableObject)this).Drawer as BattleSideDrawer;
        public ITableEventVoid<BattleKillAttemptArgs> OnDeathsDoor => _onDeathsDoor;

        public override string TableName => isMe ? Player.Name : "Противник";
        public override string TableNameDebug => isMe ? "player" : "enemy";

        public BattleSleeve Sleeve => _sleeve;
        public CardDeck Deck => _deck;

        public int HealthAtStart => _healthAtStart;
        public int GoldAtStart => _goldAtStart;
        public int EtherAtStart => _etherAtStart;

        public bool IsKilled => _isKilled;
        public bool CanBeKilled
        {
            get => _canBeKilledCounter >= 0;
            set
            {
                if (value)
                    _canBeKilledCounter++;
                else _canBeKilledCounter--;
            }
        }

        public readonly bool isMe;
        public readonly BattleAI ai;
        public TableStat Health => _health;
        public TableStat Gold => _gold;
        public TableStat Ether => _ether;

        readonly TableStat _health;
        readonly TableStat _gold;
        readonly TableStat _ether;

        readonly BattleSideFinder _finder;
        readonly BattleTerritory _territory;
        readonly TableEventVoid<BattleKillAttemptArgs> _onDeathsDoor;

        readonly int _fieldsYIndex;
        readonly int _healthAtStart;
        readonly int _goldAtStart;
        readonly int _etherAtStart;

        CardDeck _deck;
        BattleSleeve _sleeve;
        int _canBeKilledCounter;
        bool _isKilled;
        bool _killBlock;

        public BattleSide(BattleTerritory territory, bool isMe) : base(territory.Transform)
        {
            this.isMe = isMe;
            this.ai = new BattleAI(this);

            _finder = new BattleSideFinder(this);
            _territory = territory;
            _onDeathsDoor = new TableEventVoid<BattleKillAttemptArgs>();
            _fieldsYIndex = isMe ? BattleTerritory.PLAYER_FIELDS_Y : BattleTerritory.ENEMY_FIELDS_Y;
            _deck = DeckCreator();

            _healthAtStart = HealthAtStartFunc();
            _goldAtStart = GoldAtStartFunc();
            _etherAtStart = EtherAtStartFunc();

            _health = new TableStat("health", this, _healthAtStart);
            _health.OnPreSet.Add(null, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _health.OnPostSet.Add(null, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _health.OnPostSet.Add(null, OnHealthPostSet);

            _gold = new TableStat("gold", this, _goldAtStart);
            _gold.OnPreSet.Add(null, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _gold.OnPostSet.Add(null, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            _ether = new TableStat("ether", this, _etherAtStart);
            _ether.OnPreSet.Add(null, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _ether.OnPostSet.Add(null, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            AddOnInstantiatedAction(GetType(), typeof(BattleSide), () => CreateSleeve(Drawer?.transform));
        }
        protected BattleSide(BattleSide src, BattleSideCloneArgs args) : base(src)
        {
            isMe = src.isMe;
            ai = new BattleAI(this);

            _finder = new BattleSideFinder(this);
            _territory = args.srcSideTerritoryClone;
            _onDeathsDoor = (TableEventVoid<BattleKillAttemptArgs>)src._onDeathsDoor.Clone();

            _fieldsYIndex = src._fieldsYIndex;
            _deck = DeckCloner(src._deck);
            _sleeve = SleeveCloner(src._sleeve, args);

            _healthAtStart = src._healthAtStart;
            _goldAtStart = src._goldAtStart;
            _etherAtStart = src._etherAtStart;

            TableStatCloneArgs statCArgs = new(this, args.terrCArgs);
            _health = (TableStat)src._health.Clone(statCArgs);
            _gold = (TableStat)src._gold.Clone(statCArgs);
            _ether = (TableStat)src._ether.Clone(statCArgs);

            TryOnInstantiatedAction(GetType(), typeof(BattleSide));
        }

        public bool Equals(BattleSide other)
        {
            return isMe == other.isMe;
        }
        public override void Dispose()
        {
            base.Dispose();
            _health.OnPreSet.Clear();
            _health.OnPostSet.Clear();
            _gold.OnPreSet.Clear();
            _gold.OnPostSet.Clear();
            _ether.OnPreSet.Clear();
            _ether.OnPostSet.Clear();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is BattleSideCloneArgs cArgs)
                 return new BattleSide(this, cArgs);
            else return null;
        }

        public void CreateSleeve(Transform parent)
        {
            _sleeve = SleeveCreator(parent);
        }
        public void DestroySleeve(bool instantly)
        {
            _sleeve.DestroyDrawer(instantly);
            _sleeve = null;
        }

        public IEnumerable<BattleField> Fields()
        {
            for (int i = 0; i < _territory.Grid.x; i++)
            {
                BattleField field = _territory.Field(i, _fieldsYIndex);
                if (field != null) yield return field;
            }
        }
        public bool Owns(BattleFieldCard card)
        {
            if (card != null)
                 return card.Side.isMe == isMe;
            else return false;
        }
        public bool Owns(BattleField field)
        {
            if (field != null)
                 return field.Side.isMe == isMe;
            else return false;
        }

        public bool CanAfford(ITableCard card)
        {
            return CanAfford(card?.Data);
        }
        public bool CanAfford(Card card)
        {
            if (card == null) return false;
            if (card.price.currency == CardBrowser.GetCurrency("gold"))
                 return _gold >= card.price.value;
            else return _ether >= card.price.value;
        }

        public int GetCurrencyDifference(ITableCard card)
        {
            return GetCurrencyDifference(card?.Data);
        }
        public int GetCurrencyDifference(Card card)
        {
            if (card == null) return 0;
            if (card.price.currency == CardBrowser.GetCurrency("gold"))
                return card.price.value - _gold;
            else return card.price.value - _ether;
        }

        public void Purchase(ITableCard card)
        {
            Purchase(card?.Data);
        }
        public void Purchase(Card card)
        {
            if (card == null) return;
            if (card.price.currency == CardBrowser.GetCurrency("gold"))
                 _gold.AdjustValue(-card.price.value, this);
            else _ether.AdjustValue(-card.price.value, this);
        }

        public float CalculateWeight(params int[] excludedWeights)
        {
            float baseWeight = CalculateBaseWeight(excludedWeights);
            float goldWeight = CalculateCurrencyWeight("gold", excludedWeights);
            float etherWeight = CalculateCurrencyWeight("ether", excludedWeights);
            float sum = baseWeight + goldWeight + etherWeight;
            return sum;
        }
        public float CalculateBaseWeight(params int[] excludedWeights)
        {
            if (_isKilled)
                return -int.MaxValue;
            List<BattleWeight> allWeights = new();
            float healthWeight = _health;
            IEnumerable<IBattleCard> sleeveCards = Sleeve.Cards;
            IBattleCard[] affordableCards = sleeveCards.Where(c => CanAfford(c)).ToArray();
            foreach (IBattleCard card in affordableCards)
            {
                if (excludedWeights.Contains(card.Guid)) continue;
                BattleWeight weight = card.CalculateWeight(excludedWeights);
                float multiplier = 4f;
                weight.absolute /= multiplier;
                weight.relative /= multiplier;
                allWeights.Add(weight);
            }
            allWeights.AddRange(Fields().WithCard().Select(f => f.Card.CalculateWeight(excludedWeights)));
            return BattleWeight.Float(healthWeight, allWeights);
        }
        public float CalculateCurrencyWeight(string currencyId, params int[] excludedWeights)
        {
            if (_isKilled)
                return 0;
            TableStat currencyStat = currencyId == "gold" ? Gold : Ether;
            List<BattleWeight> allWeights = new();
            IEnumerable<IBattleCard> sleeveCards = Sleeve.Cards;
            IBattleCard[] currencyCards = sleeveCards.Where(c => c.Data.price.currency.id == currencyStat.Id).ToArray();
            int cardsCurrencySum = currencyCards.Sum(c => c.Price);
            float multiplier = 0.5f * (currencyStat / (float)cardsCurrencySum).Clamped(-1, 1);
            foreach (IBattleCard card in currencyCards)
            {
                if (excludedWeights.Contains(card.Guid)) continue;
                BattleWeight weight = card.CalculateWeight(excludedWeights);
                weight.absolute *= multiplier;
                weight.relative *= multiplier;
                allWeights.Add(weight);
            }
            return BattleWeight.Float(0, allWeights);
        }

        public async UniTask TryKill(BattleKillMode mode, ITableEntrySource source)
        {
            if (_killBlock) return;
            if (_isKilled) return;

            _killBlock = true;
            int damage = ((int)_health).ClampedMin(0);
            if (_health > 0)
                await _health.SetValue(0, source);
            _killBlock = false;

            BattleKillAttemptArgs args = new(this, damage, null, mode, source);
            Drawer?.transform.DOAShake();
            await _onDeathsDoor.Invoke(this, args);
            if (_health > 0 || args.handled) return;

            _isKilled = true;
            await _territory.TryConclude();
        }

        protected virtual int HealthAtStartFunc() => (_deck.Points / 4).Ceiling();
        protected virtual int GoldAtStartFunc() => 0;
        protected virtual int EtherAtStartFunc() => 0;

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new BattleSideDrawer(this, parent);
        }
        protected virtual BattleSleeve SleeveCreator(Transform parent)
        {
            return new BattleSleeve(this, parent);
        }
        protected virtual BattleSleeve SleeveCloner(BattleSleeve src, BattleSideCloneArgs args)
        {
            List<Card> srcSleeveHoldingCardsClones = new(src.Count);

            foreach (IBattleSleeveCard card in src)
                srcSleeveHoldingCardsClones.Add((Card)card.Data.Clone());

            BattleSleeveCloneArgs sleeveCArgs = new(this, args.terrCArgs);
            return (BattleSleeve)src.Clone(sleeveCArgs);
        }

        // used for debug-logging
        protected virtual UniTask OnStatPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            string sideName = side.TableNameDebug;
            string statName = stat.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile("terr", $"{sideName}: {statName}: OnPreSet: delta: {e.deltaValue} (by: {sourceName}).");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnStatPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            string sideName = side.TableNameDebug;
            string statName = stat.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile("terr", $"{sideName}: {statName}: OnPostSet: delta: {e.totalDeltaValue} (by: {sourceName}).");
            return UniTask.CompletedTask;
        }
        // ----

        protected virtual CardDeck DeckCreator()
        {
            return new CardDeck();
        }
        protected CardDeck DeckCloner(CardDeck src)
        {
            return (CardDeck)src.Clone();
        }

        protected override void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            BattleSide side = (BattleSide)sender;
            side.Sleeve?.CreateDrawer(side.Drawer.transform);
        }
        protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            BattleSide side = (BattleSide)sender;
            side.Sleeve?.DestroyDrawer(Drawer?.IsDestroyed ?? true);
        }

        private UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            if (e.newStatValue > 0)
                return UniTask.CompletedTask;

            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;
            return side.TryKill(BattleKillMode.Default, e.source);
        }
    }
}
