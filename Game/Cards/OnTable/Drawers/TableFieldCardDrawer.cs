using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Palette;
using Game.Traits;
using GreenOne;
using MyBox;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableFieldCardDrawer : TableCardDrawer
    {
        const float BG_ALPHA_MAX = 0.8f;

        public TableTraitListSetDrawer Traits => attached.Traits.Drawer;
        public readonly new TableFieldCard attached;

        readonly SpriteRenderer _bgRenderer;
        Tween _bgTween;

        public TableFieldCardDrawer(TableFieldCard card, Transform parent) : base(card, parent)
        {
            attached = card;
            attached.price.OnPostSet.Add(OnPriceStatPostSet);
            attached.moxie.OnPostSet.Add(OnMoxieStatPostSet);
            attached.health.OnPostSet.Add(OnHealthStatPostSet);
            attached.strength.OnPostSet.Add(OnStrengthStatPostSet);
            attached.Traits.Passives.OnStacksChanged.Add(OnTraitsStacksChanged);
            attached.Traits.Actives.OnStacksChanged.Add(OnTraitsStacksChanged);

            OnMouseScrollUp += OnMouseScrollUpBase;
            OnMouseScrollDown += OnMouseScrollDownBase;

            _bgRenderer = transform.Find<SpriteRenderer>("BG");
            _bgTween = Utils.emptyTween;

            RedrawHealth(card.Data.health, true);
            RedrawStrength(card.Data.strength, true);
            RedrawMoxie(card.Data.moxie, true);
            RedrawOutlineInstantly();
        }

        public override void SetCollider(bool value)
        {
            base.SetCollider(value);
            Traits?.SetCollider(value);
        }
        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            _bgRenderer.sortingOrder = value + 2;
            Traits?.SetSortingOrder(value + 3);
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            if (_bgRenderer.color.a != 0)
                _bgRenderer.SetAlpha(value * BG_ALPHA_MAX);
            Traits.SetAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            if (_bgRenderer.color.a != 0)
                _bgRenderer.color = value * BG_ALPHA_MAX;
            Traits.SetColor(value);
        }

        public void RedrawHealthAsDefault()
        {
            RedrawHealth(attached.Data.health);
        }
        public void RedrawStrengthAsDefault()
        {
            RedrawStrength(attached.Data.strength);
        }
        public void RedrawMoxieAsDefault()
        {
            RedrawMoxie(attached.Data.moxie);
        }

        public void RedrawHealthAsCurrent()
        {
            RedrawHealth(attached.health);
        }
        public void RedrawStrengthAsCurrent()
        {
            RedrawStrength(attached.strength);
        }
        public void RedrawMoxieAsCurrent()
        {
            RedrawMoxie(attached.moxie);
        }

        public void RedrawHealth(int health, bool instantly = false)
        {
            lowerLeftIcon.RedrawSprite(lHealthIconSprite);
            if (instantly)
                 lowerLeftIcon.RedrawText(health);
            else lowerLeftIcon.AnimTextNumberDelta(health);
        }
        public void RedrawStrength(int strength, bool instantly = false)
        {
            lowerRightIcon.RedrawSprite(lStrengthIconSprite);
            if (instantly)
                lowerRightIcon.RedrawText(strength);
            else lowerRightIcon.AnimTextNumberDelta(strength);
        }
        public void RedrawMoxie(int moxie, bool instantly = false)
        {
            upperRightIcon.RedrawSprite(uMoxieIconSprite);
            upperRightIcon.RedrawChunks(moxie);
        }

        public void ShowBg()
        {
            _bgTween.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(BG_ALPHA_MAX), 0.25f);
        }
        public void HideBg()
        {
            _bgTween.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(0f), 0.25f);
        }

        public void ShowBgInstantly()
        {
            _bgTween.Kill();
            _bgRenderer.color = Color.white.WithAlpha(BG_ALPHA_MAX);
        }
        public void HideBgInstantly()
        {
            _bgTween.Kill();
            _bgRenderer.color = Color.white.WithAlpha(0f);
        }

        protected virtual bool RedrawRangeFlipY() => true;
        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            attached.price.OnPostSet.Remove(OnPriceStatPostSet);
            attached.moxie.OnPostSet.Remove(OnMoxieStatPostSet);
            attached.health.OnPostSet.Remove(OnHealthStatPostSet);
            attached.strength.OnPostSet.Remove(OnStrengthStatPostSet);
            attached.Traits.Passives.OnStacksChanged.Remove(OnTraitsStacksChanged);
            attached.Traits.Actives.OnStacksChanged.Remove(OnTraitsStacksChanged);
        }

        protected override void OnUpperLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);

            int priceDefault = attached.Data.price.value;
            int priceCurrent = attached.price;
            Tooltip.Show($"Валюта: {priceCurrencyStr}\nПо умолчанию: {priceDefault} ед.\nТекущее: {priceCurrent} ед.\n<color=grey><i>Стоимость: цена установки на территорию.");
        }
        protected override void OnUpperRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int moxieDefault = attached.Data.moxie;
            int moxieCurrent = attached.moxie;
            Tooltip.Show($"По умолчанию: {moxieDefault} ед.\nТекущее: {moxieCurrent} ед.\n<color=grey><i>Инициатива: определяет быстроту действий.");
        }
        protected override void OnLowerLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int healthDefault = attached.Data.health;
            int healthCurrent = attached.health;
            Tooltip.Show($"По умолчанию: {healthDefault} ед.\nТекущее: {healthCurrent} ед.\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.");
        }
        protected override void OnLowerRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int strengthDefault = attached.Data.strength;
            int strengthCurrent = attached.strength;
            Tooltip.Show($"По умолчанию: {strengthDefault} ед.\nТекущее: {strengthCurrent} ед.\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.");
        }

        protected override void OnUpperLeftIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }
        protected override void OnUpperRightIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }
        protected override void OnLowerLeftIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }
        protected override void OnLowerRightIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;
            Traits?.ShowStoredElementsInstantly();
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            if (e.handled) return;
            Traits?.HideStoredElementsInstantly();
        }
        protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
        {
            e.handled = Traits.elements.IsAnySelected;
            base.OnMouseClickLeftBase(sender, e);
            if (e.handled) return;
        }
        protected override OutlineType GetOutlineType()
        {
            TableTraitListSet traits = attached.Traits;
            bool hasPassives = traits.Passives.Count != 0;
            bool hasActives = traits.Actives.Count != 0;

            if (hasPassives == hasActives)
            {
                if (hasPassives)
                     return OutlineType.Both;
                else return OutlineType.None;
            }
            if (hasPassives)
                 return OutlineType.Passive;
            else return OutlineType.Active;
        }

        UniTask OnTraitsStacksChanged(object sender, TableTraitStacksSetArgs e)
        {
            TableTraitList list = (TableTraitList)sender;
            TableFieldCard card = list.Set.Owner;

            TableFieldCardDrawer drawer = card.Drawer;
            drawer?.RedrawOutline();
            return UniTask.CompletedTask;
        }
        UniTask OnPriceStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.RedrawPriceAsDefault();
            return UniTask.CompletedTask;
        }
        UniTask OnMoxieStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.RedrawMoxieAsCurrent();
            return UniTask.CompletedTask;
        }
        UniTask OnHealthStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            TableFieldCardDrawer drawer = card.Drawer;
            drawer?.RedrawHealthAsCurrent();
            return UniTask.CompletedTask;
        }
        UniTask OnStrengthStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            TableFieldCardDrawer drawer = card.Drawer;
            drawer?.RedrawStrengthAsCurrent();
            return UniTask.CompletedTask;
        }
    }
}
