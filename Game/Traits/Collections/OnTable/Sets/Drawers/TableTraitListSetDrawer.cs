using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using GreenOne;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableTraitListSet"/>.
    /// </summary>
    public sealed class TableTraitListSetDrawer : ScrollableDrawer, IEnumerable<TableTraitListElementDrawer>
    {
        public TableFieldCardDrawer Owner => attached.Owner.Drawer;
        public bool IsAnySelected => IsAnyElementSelected();
        public readonly new TableTraitListSet attached;
        public readonly TableTraitListSetDrawerElementsQueue queue;

        static readonly GameObject _prefab;
        Tween _animAlphaTween;

        static TableTraitListSetDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Traits/Trait list set");
        }
        public TableTraitListSetDrawer(TableTraitListSet set) : base(set, _prefab, set.Owner.Drawer.transform)
        {
            gameObject.name = "Traits";
            attached = set;
            queue = new TableTraitListSetDrawerElementsQueue(this);

            attached.OnDrawerCreated += OnSetDrawerCreated;
            SetSortingOrder(set.Owner.Drawer.SortingOrder + 8);
            gameObject.SetActive(false);
        }

        public Tween ShowStoredElements()
        {
            if (gameObject.activeSelf && !_animAlphaTween.IsActive() && Alpha == 1) 
                return _animAlphaTween;

            gameObject.SetActive(true);
            attached.Owner.Drawer.ShowBg();
            _animAlphaTween.Kill();
            _animAlphaTween = this.DOFade(1, 0.25f);
            return _animAlphaTween;
        }
        public Tween HideStoredElements()
        {
            if (!gameObject.activeSelf || (!_animAlphaTween.IsActive() && Alpha == 0)) 
                return _animAlphaTween;

            attached.Owner.Drawer.HideBg();
            _animAlphaTween.Kill();
            _animAlphaTween = this.DOFade(0, 0.25f);
            _animAlphaTween.OnComplete(gameObject.Deactivate);
            return _animAlphaTween;
        }

        public void ShowStoredElementsInstantly()
        {
            if (queue.IsEmpty) return;
            if (gameObject.activeSelf) return;

            Alpha = 1f;
            gameObject.SetActive(true);
            _animAlphaTween.Kill();
        }
        public void HideStoredElementsInstantly()
        {
            if (queue.IsEmpty) return;
            if (!gameObject.activeSelf) return;

            Alpha = 0f;
            gameObject.SetActive(false);
            _animAlphaTween.Kill();
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();
            _animAlphaTween.Kill();
            foreach (TableTraitListElementDrawer drawer in queue)
                drawer?.TryDestroyInstantly();
        }
        protected override UniTask DestroyAnimated()
        {
            UniTask task = base.DestroyAnimated();
            _animAlphaTween.Kill();
            foreach (TableTraitListElementDrawer drawer in queue)
                drawer.TryDestroyAnimated();
            return task;
        }

        protected override bool SetActiveStateOnAlphaSet() => false;
        protected override bool IgnoreMouseScroll()
        {
            return base.IgnoreMouseScroll() || queue.IsRunning;
        }
        protected override bool CanBeSelected()
        {
            return base.CanBeSelected() && !Sleeves.ITableSleeveCard.IsHoldingAnyCard;
        }

        bool IsAnyElementSelected()
        {
            foreach (TableTraitListElementDrawer drawer in queue)
            {
                if (drawer != null && drawer.IsSelected)
                    return true;
            }
            return false;
        }
        void OnSetDrawerCreated(object sender, EventArgs e)
        {
            TableTraitListSet set = (TableTraitListSet)sender;

            // saves traits order instead of passing [passives[], actives[]]
            foreach (ITableTraitListElement element in set.Actives)
            {
                element.CreateDrawer(transform);
                set.Drawer.queue.EnqueueInstantly(element);
            }
            foreach (ITableTraitListElement element in set.Passives)
            {
                element.CreateDrawer(transform);
                set.Drawer.queue.EnqueueInstantly(element);
            }

            attached.OnDrawerCreated -= OnSetDrawerCreated;
        }

        public IEnumerator<TableTraitListElementDrawer> GetEnumerator()
        {
            return queue.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }
    }
}
