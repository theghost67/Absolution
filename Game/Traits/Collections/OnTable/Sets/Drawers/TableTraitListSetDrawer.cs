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
    // TODO: implement scrolling
    // TODO: set sorting orders for masks

    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableTraitListSet"/>.
    /// </summary>
    public sealed class TableTraitListSetDrawer : Drawer, IEnumerable<TableTraitListElementDrawer>
    {
        const float VIEWPORT_HEIGHT = 0.69f;
        public TableFieldCardDrawer Owner => attached.Owner.Drawer;

        public readonly new TableTraitListSet attached;
        public readonly TableTraitListSetDrawerElementsCollection elements;

        Tween _animScrollTween;
        Tween _animAlphaTween;
        float _animAlpha;

        public TableTraitListSetDrawer(TableTraitListSet set) : base(set, set.Owner.Drawer.transform.CreateEmptyObject("Traits"))
        {
            attached = set;
            elements = new TableTraitListSetDrawerElementsCollection(this);

            attached.OnDrawerCreated += OnSetDrawerCreated;
            SetSortingOrder(set.Owner.Drawer.GetSortingOrder() + 8);
            gameObject.SetActive(false);
        }

        public override void SetCollider(bool value)
        {
            base.SetCollider(value);
            foreach (TableTraitListElementDrawer drawer in elements)
                drawer.SetCollider(value);
        }
        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            foreach (TableTraitListElementDrawer drawer in elements)
                drawer.SetSortingOrder(value);
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            _animAlpha = value;
            foreach (TableTraitListElementDrawer drawer in elements)
                drawer.SetAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            foreach (TableTraitListElementDrawer drawer in elements)
                drawer.SetColor(value);
        }

        public Tween ShowStoredElements()
        {
            if (!elements.ContainsTraits) return _animAlphaTween;
            if (gameObject.activeSelf && !_animAlphaTween.active && _animAlpha == 1) return _animAlphaTween;

            gameObject.SetActive(true);
            attached.Owner.Drawer.ShowBg();
            _animAlphaTween.Kill();
            _animAlphaTween = DOVirtual.Float(_animAlpha, 1, 0.25f, SetAlpha);
            return _animAlphaTween;
        }
        public Tween HideStoredElements()
        {
            if (!elements.ContainsTraits) return _animAlphaTween;
            if (!gameObject.activeSelf || (!_animAlphaTween.active && _animAlpha == 0)) return _animAlphaTween;

            attached.Owner.Drawer.HideBg();
            _animAlphaTween.Kill();
            _animAlphaTween = DOVirtual.Float(_animAlpha, 0, 0.25f, SetAlpha);
            _animAlphaTween.OnComplete(gameObject.Deactivate);
            return _animAlphaTween;
        }
        public Tween ScrollStoredElements(float posY)
        {
            // TODO: implement
            return _animScrollTween;
        }

        public void ShowStoredElementsInstantly()
        {
            if (!elements.ContainsTraits) return;
            if (gameObject.activeSelf) return;

            gameObject.SetActive(true);
            _animAlphaTween.Kill();
            SetAlpha(1f);
            attached.Owner.Drawer.ShowBgInstantly();
        }
        public void HideStoredElementsInstantly()
        {
            if (!elements.ContainsTraits) return;
            if (!gameObject.activeSelf) return;

            gameObject.SetActive(false);
            _animAlphaTween.Kill();
            SetAlpha(0f);
            attached.Owner.Drawer.HideBgInstantly();
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();
            AnimKill();
            foreach (TableTraitListElementDrawer drawer in elements)
                drawer.TryDestroyInstantly();
        }
        protected override UniTask DestroyAnimated()
        {
            UniTask task = base.DestroyAnimated();
            AnimKill();
            foreach (TableTraitListElementDrawer drawer in elements)
                drawer.TryDestroyAnimated();
            return task;
        }
        protected override bool SetActiveStateOnAlphaSet() => false;

        void OnSetDrawerCreated(object sender, EventArgs e)
        {
            TableTraitListSet set = (TableTraitListSet)sender;

            // saves traits order instead of passing [passives[], actives[]]
            foreach (ITableTraitListElement element in set.Actives)
            {
                element.CreateDrawer(transform);
                set.Drawer.elements.EnqueueInstantly(element);
            }
            foreach (ITableTraitListElement element in set.Passives)
            {
                element.CreateDrawer(transform);
                set.Drawer.elements.EnqueueInstantly(element);
            }

            attached.OnDrawerCreated -= OnSetDrawerCreated;
        }
        void AnimKill()
        {
            _animScrollTween.Kill();
            _animAlphaTween.Kill();
        }

        public IEnumerator<TableTraitListElementDrawer> GetEnumerator()
        {
            return elements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }
}
