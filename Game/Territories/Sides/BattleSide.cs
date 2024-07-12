using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Environment;
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
    public class BattleSide : TableObject, ITableEntrySource, ICloneableWithArgs, IEquatable<BattleSide>
    {
        public BattleTerritory Territory => _territory;
        public BattleSide Opposite => _territory.GetOppositeOf(this);
        public TableFinder Finder => _finder;
        public new BattleSideDrawer Drawer => ((TableObject)this).Drawer as BattleSideDrawer;

        public override string TableName => isMe ? Player.Name : "Противник";
        public override string TableNameDebug => isMe ? "player" : "enemy";

        public BattleSleeve Sleeve => _sleeve;
        public CardDeck Deck => _deck;
        public float Weight => CalculateWeight();

        public virtual int HealthAtStart => (_deck.Points / 4).Ceiling();
        public virtual int GoldAtStart => 0;
        public virtual int EtherAtStart => 0;

        public readonly bool isMe;
        public readonly BattleAI ai;
        public readonly TableStat health;
        public readonly TableStat gold;
        public readonly TableStat ether;

        readonly BattleSideFinder _finder;
        readonly BattleTerritory _territory;
        readonly int _fieldsYIndex;
        readonly string _eventsGuid;

        CardDeck _deck;
        BattleSleeve _sleeve;

        public BattleSide(BattleTerritory territory, bool isMe) : base(territory.Transform)
        {
            this.ai = new BattleAI(this);
            this.isMe = isMe;

            _finder = new BattleSideFinder(this);
            _territory = territory;
            _fieldsYIndex = isMe ? BattleTerritory.PLAYER_FIELDS_Y : BattleTerritory.ENEMY_FIELDS_Y;
            _eventsGuid = this.GuidStrForEvents(1);
            _deck = DeckCreator();

            health = new TableStat(nameof(health), this, HealthAtStart);
            health.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP);
            health.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP);

            gold = new TableStat(nameof(gold), this, GoldAtStart);
            gold.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP);
            gold.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP);

            ether = new TableStat(nameof(ether), this, EtherAtStart);
            ether.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP);
            ether.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP);

            AddOnInstantiatedAction(GetType(), typeof(BattleSide), () => CreateSleeve(Drawer?.transform));
        }
        protected BattleSide(BattleSide src, BattleSideCloneArgs args) : base(src)
        {
            ai = new BattleAI(this);
            isMe = src.isMe;

            _finder = new BattleSideFinder(this);
            _territory = args.srcSideTerritoryClone;
            _fieldsYIndex = src._fieldsYIndex;
            _deck = DeckCloner(src._deck);
            _sleeve = SleeveCloner(src._sleeve, args);

            TableStatCloneArgs statCArgs = new(this, args.terrCArgs);
            health = (TableStat)src.health.Clone(statCArgs);
            gold = (TableStat)src.gold.Clone(statCArgs);
            ether = (TableStat)src.ether.Clone(statCArgs);

            TryOnInstantiatedAction(GetType(), typeof(BattleSide));
        }

        public bool Equals(BattleSide other)
        {
            return isMe == other.isMe;
        }
        public override void Dispose()
        {
            base.Dispose();
            health.OnPreSet.Clear();
            health.OnPostSet.Clear();
            gold.OnPreSet.Clear();
            gold.OnPostSet.Clear();
            ether.OnPreSet.Clear();
            ether.OnPostSet.Clear();
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
                 return gold >= card.price.value;
            else return ether >= card.price.value;
        }

        public int GetCurrencyDifference(ITableCard card)
        {
            return GetCurrencyDifference(card?.Data);
        }
        public int GetCurrencyDifference(Card card)
        {
            if (card == null) return 0;
            if (card.price.currency == CardBrowser.GetCurrency("gold"))
                return card.price.value - gold;
            else return card.price.value - ether;
        }

        public void Purchase(ITableCard card)
        {
            Purchase(card?.Data);
        }
        public void Purchase(Card card)
        {
            if (card == null) return;
            if (card.price.currency == CardBrowser.GetCurrency("gold"))
                 gold.AdjustValue(-card.price.value, this);
            else ether.AdjustValue(-card.price.value, this);
        }

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
            return Traveler.NewDeck();
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

        float CalculateWeight()
        {
            if (health <= 0)
                return -int.MaxValue;
            IEnumerable<BattleWeight> weights = Fields().WithCard().Select(f => f.Card.Weight);
            return BattleWeight.Float(health, weights);
        }
    }
}
