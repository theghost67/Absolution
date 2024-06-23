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
    public class BattleSide : Unique, ITableDrawable, ITableEntrySource, ICloneableWithArgs, IEquatable<BattleSide>
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public BattleTerritory Territory => _territory;
        public BattleSide Opposite => _territory.GetOppositeOf(this);
        public BattleSideDrawer Drawer => _drawer;
        public TableFinder Finder => _finder;
        public string TableName => isMe ? Player.Name : "Противник";

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
        readonly CardDeck _deck;
        readonly BattleTerritory _territory;
        readonly int _fieldsYIndex;

        BattleSleeve _sleeve;
        BattleSideDrawer _drawer;
        Drawer ITableDrawable.Drawer => _drawer;

        public BattleSide(BattleTerritory territory, bool isMe, bool withDrawer = true)
        {
            OnDrawerCreated += OnDrawerCreatedBase;
            OnDrawerDestroyed += OnDrawerDestroyedBase;

            this.ai = new BattleAI(this);
            this.isMe = isMe;

            _finder = new BattleSideFinder(this);
            _territory = territory;
            _fieldsYIndex = isMe ? BattleTerritory.PLAYER_FIELDS_Y : BattleTerritory.ENEMY_FIELDS_Y;
            _deck = DeckCreator();
            _sleeve = SleeveCreator(null);

            health = new TableStat(this, HealthAtStart);
            gold = new TableStat(this, GoldAtStart);
            ether = new TableStat(this, EtherAtStart);

            health.OnPreSet.Add(OnHealthPreSetBase_TOP);
            gold.OnPreSet.Add(OnGoldPreSetBase_TOP);
            ether.OnPreSet.Add(OnEtherPreSetBase_TOP);

            health.OnPostSet.Add(OnHealthPostSetBase_TOP);
            gold.OnPostSet.Add(OnGoldPostSetBase_TOP);
            ether.OnPostSet.Add(OnEtherPostSetBase_TOP);

            if (withDrawer)
                CreateDrawer(_territory.transform);
        }
        protected BattleSide(BattleSide src, BattleSideCloneArgs args) : base(src.Guid)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

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
        }

        public bool Equals(BattleSide other)
        {
            return isMe == other.isMe;
        }
        public void CreateDrawer(Transform parent)
        {
            if (_drawer != null) return;
            BattleSideDrawer drawer = DrawerCreator(parent);
            DrawerSetter(drawer);
            OnDrawerCreated?.Invoke(this, EventArgs.Empty);
        }
        public void DestroyDrawer(bool instantly)
        {
            if (_drawer == null) return;
            _drawer.TryDestroy(instantly);
            DrawerSetter(null);
            OnDrawerDestroyed?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            DestroyDrawer(true);
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

        public IEnumerable<BattleField> Fields()
        {
            for (int i = 0; i < _territory.grid.x; i++)
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
                 gold.AdjustValueAbs(-card.price.value, this);
            else ether.AdjustValueAbs(-card.price.value, this);
        }

        protected virtual void DrawerSetter(BattleSideDrawer value)
        {
            _drawer = value;
        }
        protected virtual BattleSideDrawer DrawerCreator(Transform parent)
        {
            return new BattleSideDrawer(this, parent);
        }

        protected virtual BattleSleeve SleeveCreator(Transform parent)
        {
            return new BattleSleeve(this, parent, _drawer != null);
        }
        protected virtual BattleSleeve SleeveCloner(BattleSleeve src, BattleSideCloneArgs args)
        {
            List<Card> srcSleeveHoldingCardsClones = new(src.Count);

            foreach (IBattleSleeveCard card in src)
                srcSleeveHoldingCardsClones.Add((Card)card.Data.Clone());

            BattleSleeveCloneArgs sleeveCArgs = new(this, args.terrCArgs);
            return (BattleSleeve)src.Clone(sleeveCArgs);
        }

        // used for logging
        protected virtual UniTask OnHealthPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            if (e.source != null)
                 side.Territory.WriteLogForDebug($"{side.TableName}: здоровье: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else side.Territory.WriteLogForDebug($"{side.TableName}: здоровье: попытка изменения на {e.deltaValue} ед.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnGoldPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            if (e.source != null)
                 side.Territory.WriteLogForDebug($"{side.TableName}: золото: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else side.Territory.WriteLogForDebug($"{side.TableName}: золото: попытка изменения на {e.deltaValue} ед.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEtherPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            if (e.source != null)
                 side.Territory.WriteLogForDebug($"{side.TableName}: эфир: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else side.Territory.WriteLogForDebug($"{side.TableName}: эфир: попытка изменения на {e.deltaValue} ед.");
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnHealthPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            if (e.source != null)
                 side.Territory.WriteLogForDebug($"{side.TableName}: здоровье: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else side.Territory.WriteLogForDebug($"{side.TableName}: здоровье: изменение на {e.totalDeltaValue} ед.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnGoldPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            if (e.source != null)
                 side.Territory.WriteLogForDebug($"{side.TableName}: золото: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else side.Territory.WriteLogForDebug($"{side.TableName}: золото: изменение на {e.totalDeltaValue} ед.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEtherPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;

            if (e.source != null)
                 side.Territory.WriteLogForDebug($"{side.TableName}: эфир: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else side.Territory.WriteLogForDebug($"{side.TableName}: эфир: изменение на {e.totalDeltaValue} ед.");
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

        void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            BattleSide side = (BattleSide)sender;
            side.Sleeve.CreateDrawer(side.Drawer.transform);
        }
        void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            BattleSide side = (BattleSide)sender;
            side.Sleeve.DestroyDrawer(true);
        }

        float CalculateWeight()
        {
            if (health <= 0) return 0;
            IEnumerable<BattleWeight> weights = Fields().WithCard().Select(f => f.Card.Weight);
            return BattleWeight.Float(health, weights);
        }
    }
}
