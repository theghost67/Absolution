using Cysharp.Threading.Tasks;
using Game.Traits;
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
        public readonly TableFieldCardDrawerQueue queue;
        readonly string _eventsGuid;

        public TableFieldCardDrawer(TableFieldCard card, Transform parent) : base(card, parent, redrawIcons: false)
        {
            attached = card;
            queue = new TableFieldCardDrawerQueue(this);
            _eventsGuid = this.GuidGen(2);

            attached.price.OnPostSet.Add(_eventsGuid, OnPriceStatPostSet);
            attached.moxie.OnPostSet.Add(_eventsGuid, OnMoxieStatPostSet);
            attached.health.OnPostSet.Add(_eventsGuid, OnHealthStatPostSet);
            attached.strength.OnPostSet.Add(_eventsGuid, OnStrengthStatPostSet);
            attached.Traits.Passives.OnStacksChanged.Add(_eventsGuid, OnTraitsStacksChanged);
            attached.Traits.Actives.OnStacksChanged.Add(_eventsGuid, OnTraitsStacksChanged);

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
            Traits?.SetAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            Traits?.SetColor(value);
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            attached.price.OnPostSet.Remove(_eventsGuid);
            attached.moxie.OnPostSet.Remove(_eventsGuid);
            attached.health.OnPostSet.Remove(_eventsGuid);
            attached.strength.OnPostSet.Remove(_eventsGuid);
            attached.Traits.Passives.OnStacksChanged.Remove(_eventsGuid);
            attached.Traits.Actives.OnStacksChanged.Remove(_eventsGuid);
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

        protected override string UpperLeftIconTooltip()
        {
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);
            int priceDefault = attached.Data.price.value;
            return $"Валюта: {priceCurrencyStr}\nПо умолчанию: {priceDefault}.\nТекущее: {attached.price.StatToStringRich(priceDefault)} ед.\n<color=grey><i>Стоимость: цена установки на территорию.";
        }
        protected override string UpperRightIconTooltip()
        {
            int moxieDefault = attached.Data.moxie;
            int initiationOrder = (attached as BattleFieldCard)?.InitiationOrder ?? -1;
            if (initiationOrder == -1)
                 return $"По умолчанию: {moxieDefault}.\nТекущее: {attached.moxie.StatToStringRich(moxieDefault)} ед.\n<color=grey><i>Инициатива: определяет быстроту действий.";
            else return $"По умолчанию: {moxieDefault}.\nТекущее: {attached.moxie.StatToStringRich(moxieDefault)} ед.\nПозиция в очереди: {initiationOrder}.\n<color=grey><i>Инициатива: определяет быстроту действий.";
        }
        protected override string LowerLeftIconTooltip()
        {
            int healthDefault = attached.Data.health;
            return $"По умолчанию: {healthDefault} ед.\nТекущее: {attached.health.StatToStringRich(healthDefault)} ед.\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.";
        }
        protected override string LowerRightIconTooltip()
        {
            int strengthDefault = attached.Data.strength;
            return $"По умолчанию: {strengthDefault} ед.\nТекущее: {attached.strength.StatToStringRich(strengthDefault)} ед.\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.";
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;

            TableTraitListSetDrawer setDrawer = Traits;
            if (setDrawer == null) return;
            if (setDrawer.elements.IsEmpty) return;
            if (setDrawer.elements.IsRunning) return;
            if (queue.IsRunning) return;

            Traits?.ShowStoredElementsInstantly();
            if (!this.HasInitiationPreview())
                ShowBgInstantly();
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);

            TableTraitListSetDrawer setDrawer = Traits;
            if (setDrawer == null) return;
            if (setDrawer.elements.IsEmpty) return;
            if (setDrawer.elements.IsRunning) return;
            if (queue.IsRunning) return;

            setDrawer?.HideStoredElementsInstantly();
            if (!this.HasInitiationPreview())
                HideBgInstantly();
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
