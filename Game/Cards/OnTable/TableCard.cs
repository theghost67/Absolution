using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс для любой карты, находящейся на столе.
    /// </summary>
    public abstract class TableCard : Unique, ITableCard
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public Card Data => _data;
        public TableCardDrawer Drawer => _drawer;
        public virtual TableFinder Finder => null;
        public virtual string TableName => Data.name;

        public readonly TableStat price;
        readonly Card _data;
        TableCardDrawer _drawer;

        public TableCard(Card data, Transform parent, bool withDrawer = true) : base()
        {
            OnDrawerCreated += OnDrawerCreatedBase;
            OnDrawerDestroyed += OnDrawerDestroyedBase;

            _data = data;
            price = new TableStat(this, data.price.value);
            price.OnPreSet.Add(OnPricePreSetBase_TOP, 256);
            price.OnPostSet.Add(OnPricePostSetBase_TOP, 256);

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TableCard(TableCard src, TableCardCloneArgs args) : base(src.Guid)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            _data = args.srcCardDataClone;
            price = (TableStat)src.price.Clone(new TableStatCloneArgs(this, args.terrCArgs));
        }

        public virtual void Dispose()
        {
            DestroyDrawer(true);
        }
        public abstract object Clone(CloneArgs args);

        public void CreateDrawer(Transform parent)
        {
            if (_drawer != null) return;
            TableCardDrawer drawer = DrawerCreator(parent);
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

        protected virtual void DrawerSetter(TableCardDrawer value)
        {
            _drawer = value;
        }
        protected abstract TableCardDrawer DrawerCreator(Transform parent);

        protected virtual void OnDrawerCreatedBase(object sender, EventArgs e) { }
        protected virtual void OnDrawerDestroyedBase(object sender, EventArgs e) { }

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
