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
        public TableTraitListSetDrawer Traits => attached.Traits.Drawer;
        public readonly new TableFieldCard attached;

        public TableFieldCardDrawer(TableFieldCard card, Transform parent) : base(card, parent, redrawIcons: false)
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

            RedrawIcons();
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
            Traits?.SetSortingOrder(value + 3);
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            Traits.SetAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            Traits.SetColor(value);
        }

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
        protected override int UpperLeftIconDisplayValue()
        {
            return attached.price;
        }
        protected override int UpperRightIconDisplayValue()
        {
            return attached.moxie;
        }
        protected override int LowerLeftIconDisplayValue()
        {
            return attached.health;
        }
        protected override int LowerRightIconDisplayValue()
        {
            return attached.strength;
        }

        protected override void OnUpperLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);
            int priceDefault = attached.Data.price.value;
            Tooltip.Show($"Валюта: {priceCurrencyStr}\nПо умолчанию: {priceDefault}.\nТекущее: {attached.price.StatToStringRich(priceDefault)} ед.\n<color=grey><i>Стоимость: цена установки на территорию.");
        }
        protected override void OnUpperRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            int moxieDefault = attached.Data.moxie;
            int initiationOrder = (attached as BattleFieldCard)?.InitiationOrder ?? -1;
            if (initiationOrder == -1)
                 Tooltip.Show($"По умолчанию: {moxieDefault}.\nТекущее: {attached.moxie.StatToStringRich(moxieDefault)} ед.\n<color=grey><i>Инициатива: определяет быстроту действий.");
            else Tooltip.Show($"По умолчанию: {moxieDefault}.\nТекущее: {attached.moxie.StatToStringRich(moxieDefault)} ед.\nПозиция в очереди: {initiationOrder}.\n<color=grey><i>Инициатива: определяет быстроту действий.");
        }
        protected override void OnLowerLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            int healthDefault = attached.Data.health;
            Tooltip.Show($"По умолчанию: {healthDefault} ед.\nТекущее: {attached.health.StatToStringRich(healthDefault)} ед.\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.");
        }
        protected override void OnLowerRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            int strengthDefault = attached.Data.strength;
            Tooltip.Show($"По умолчанию: {strengthDefault} ед.\nТекущее: {attached.strength.StatToStringRich(strengthDefault)} ед.\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.");
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
            card.Drawer?.priceIcon.RedrawValue(e.newStatValue);
            return UniTask.CompletedTask;
        }
        UniTask OnMoxieStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.moxieIcon.RedrawValue(e.newStatValue);
            return UniTask.CompletedTask;
        }
        UniTask OnHealthStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.healthIcon.RedrawValue(e.newStatValue);
            return UniTask.CompletedTask;
        }
        UniTask OnStrengthStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.strengthIcon.RedrawValue(e.newStatValue);
            return UniTask.CompletedTask;
        }
    }
}
