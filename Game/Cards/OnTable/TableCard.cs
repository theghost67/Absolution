using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс для любой карты, находящейся на столе.
    /// </summary>
    public abstract class TableCard : TableObject, ITableCard
    {
        public Card Data => _data;
        public new TableCardDrawer Drawer => ((TableObject)this).Drawer as TableCardDrawer;
        public virtual TableFinder Finder => null;

        public override string TableName => Data.name;
        public override string TableNameDebug => $"{Data.id}[?]+{GuidStr}";

        public readonly TableStat price;
        readonly string _eventsGuid;
        readonly Card _data;

        public TableCard(Card data, Transform parent) : base(parent)
        {
            _data = data;
            _eventsGuid = this.GuidGen(1);

            price = new TableStat(nameof(price), this, data.price.value);
            price.OnPreSet.Add(_eventsGuid, OnPricePreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            price.OnPostSet.Add(_eventsGuid, OnPricePostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableCard));
        }
        protected TableCard(TableCard src, TableCardCloneArgs args) : base(src)
        {
            _data = args.srcCardDataClone;
            price = (TableStat)src.price.Clone(new TableStatCloneArgs(this, args.terrCArgs));

            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableCard));
        }

        public abstract object Clone(CloneArgs args);

        // used in BattleFieldCard for logging
        protected virtual UniTask OnPricePreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnPricePostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            return UniTask.CompletedTask;
        }
    }
}
