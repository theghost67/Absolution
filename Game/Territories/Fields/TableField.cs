using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Core;
using MyBox;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровое поле на столе с возможностью хранения карты типа <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableField : TableObject, ITableFindable, ITableLoggable, ICloneableWithArgs
    {
        public IIdEventVoidAsync OnCardAttached => _onCardAttached;
        public IIdEventVoidAsync OnCardDetatched => _onCardDetatched;

        public TableTerritory Territory => _territory;
        public TableField Opposite => _territory?.FieldOpposite(pos);
        public TableFieldCard Card => _card;
        public new TableFieldDrawer Drawer => ((TableObject)this).Drawer as TableFieldDrawer;
        public TableFinder Finder => _finder;

        public override string TableName => GetTableName();
        public override string TableNameDebug => GetTableNameDebug();

        public readonly int2 pos;
        public readonly TableStat health;

        readonly TableTerritory _territory;
        readonly TableFieldFinder _finder;
        readonly TableEventVoid _onCardAttached;
        readonly TableEventVoid _onCardDetatched;
        readonly string _eventsGuid;

        TableFieldCard _card;

        // territory can be null
        public TableField(TableTerritory territory, int2 pos, Transform parent) : base(parent)
        {
            this.pos = pos;
            _territory = territory;
            _finder = new TableFieldFinder(this);
            _onCardAttached = new TableEventVoid();
            _onCardDetatched = new TableEventVoid();
            _eventsGuid = this.GuidGen(2);

            health = new TableStat(nameof(health), this, 0);
            health.OnPostSet.Add(_eventsGuid, OnHealthPostSetBase);

            OnDrawerCreated += OnDrawerCreatedBase;
            OnDrawerDestroyed += OnDrawerDestroyedBase;

            TryOnInstantiatedAction(GetType(), typeof(TableField));
        }
        protected TableField(TableField src, TableFieldCloneArgs args) : base(src)
        {
            pos = src.pos;
            _territory = args.srcTerrClone;
            _finder = new TableFieldFinder(this);
            _onCardAttached = (TableEventVoid)src._onCardAttached.Clone();
            _onCardDetatched = (TableEventVoid)src._onCardDetatched.Clone();

            TableStatCloneArgs healthCArgs = new(this, args.terrCArgs);
            health = (TableStat)src.health.Clone(healthCArgs);

            AddOnInstantiatedAction(GetType(), typeof(TableField), () => _card = CardCloner(src.Card, args));
        }

        public override void Dispose()
        {
            base.Dispose();
            Card?.Dispose();
            Drawer?.Dispose();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableFieldCloneArgs cArgs)
                 return new TableField(this, cArgs);
            else return null;
        }

        public async UniTask AttachCard(TableFieldCard card, ITableEntrySource source)
        {
            if (_card != null) return;
            if (card == null)
                DetatchCard(source);

            _card = card;
            if (Drawer != null)
                await Drawer.AnimAttachCard(card.Drawer).AsyncWaitForCompletion();
            await _onCardAttached.Invoke(this, EventArgs.Empty);
            await card.AttachToField(this, source);
        }
        public async UniTask DetatchCard(ITableEntrySource source)
        {
            if (_card == null) return;
            TableFieldCard card = _card;

            _card = null;
            await _onCardDetatched.Invoke(this, EventArgs.Empty);
            if (Drawer != null)
                await Drawer.AnimDetatchCard(card.Drawer).AsyncWaitForCompletion();
            await card.AttachToField(null, source);
        }

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableFieldDrawer(this, parent);
        }
        protected virtual TableFieldCard CardCloner(TableFieldCard src, TableFieldCloneArgs args)
        {
            if (src == null) return null;
            TableFieldCardCloneArgs cardCArgs = new((FieldCard)src.Data.Clone(), this, args.terrCArgs);
            return (TableFieldCard)src.Clone(cardCArgs);
        }
        protected virtual UniTask OnHealthPostSetBase(object sender, TableStat.PostSetArgs e)
        {
            return UniTask.CompletedTask;
        }

        protected override void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            TableField field = (TableField)sender;
            field.Card?.CreateDrawer(Drawer.transform);
        }
        protected override void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            TableField field = (TableField)sender;
            field.Card?.DestroyDrawer(Drawer?.IsDestroyed ?? true);
        }

        string GetTableName()
        {
            if (this == null)
                return "-";
            if (this is not BattleField)
                return pos.x.ToString();

            Color color = pos.y == BattleTerritory.PLAYER_FIELDS_Y ? Color.green : Color.red;
            string fieldColorHex = color.ToHex();
            return $"<color={fieldColorHex}>{pos.x + 1}</color>";
        }
        string GetTableNameDebug()
        {
            if (this == null)
                return "-";
            if (this is not BattleField)
                return pos.x.ToString();

            string xChar = (pos.x + 1).ToString();
            string sideChar = pos.y == BattleTerritory.PLAYER_FIELDS_Y ? "p" : "e";
            return $"{xChar}{sideChar}";
        }
    }
}
