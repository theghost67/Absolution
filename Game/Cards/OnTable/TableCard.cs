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
        public TableStat Price => _price;
        public new TableCardDrawer Drawer => ((TableObject)this).Drawer as TableCardDrawer;
        public virtual TableFinder Finder => null;

        public int TurnAge { get => -1; set { return; } }
        public override string TableName => Data.name;
        public override string TableNameDebug => $"{Data.id}[?]+{GuidStr}";

        readonly string _eventsGuid;
        readonly Card _data;
        readonly TableStat _price;

        public TableCard(Card data, Transform parent) : base(parent)
        {
            _data = data;
            _eventsGuid = this.GuidGen(1);

            _price = new TableStat("price", this, data.price.value);
            _price.OnPreSet.Add(_eventsGuid, OnStatPreSetBase_TOP, TableEventVoid.TOP_PRIORITY);
            _price.OnPostSet.Add(_eventsGuid, OnStatPostSetBase_TOP, TableEventVoid.TOP_PRIORITY);

            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableCard));
        }
        protected TableCard(TableCard src, TableCardCloneArgs args) : base(src)
        {
            _data = args.srcCardDataClone;
            _price = (TableStat)src._price.Clone(new TableStatCloneArgs(this, args.terrCArgs));

            // class is abstract
            //TryOnInstantiatedAction(GetType(), typeof(TableCard));
        }

        public abstract object Clone(CloneArgs args);

        protected virtual UniTask OnStatPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnStatPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableCard owner = (TableCard)stat.Owner;
            TableCardDrawer drawer = owner.Drawer;
            if (drawer == null) return UniTask.CompletedTask;
            switch (stat.Id)
            {
                case "price": drawer.priceIcon.RedrawValue(); break;
                case "moxie": drawer.moxieIcon.RedrawValue(); break;
                case "health": drawer.healthIcon.RedrawValue(); break;
                case "strength": drawer.strengthIcon.RedrawValue(); break;
            }
            return UniTask.CompletedTask;
        }
    }
}
