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

        public TableFieldCardDrawer(TableFieldCard card, Transform parent) : base(card, parent)
        {
            attached = card;
            queue = new TableFieldCardDrawerQueue(this);
            _eventsGuid = this.GuidGen(2);

            attached.Traits.Passives.OnStacksChanged.Add(_eventsGuid, OnTraitsStacksChanged);
            attached.Traits.Actives.OnStacksChanged.Add(_eventsGuid, OnTraitsStacksChanged);

            RedrawIcons();
            RedrawOutlineInstantly();
        }

        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            TableTraitListSetDrawer traits = Traits;
            if (traits != null)
                traits.ColliderEnabled = value;
        }
        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            TableTraitListSetDrawer traits = Traits;
            if (traits != null)
                traits.SortingOrder = value + 3;
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            queue.SetColor(value);
            TableTraitListSetDrawer traits = Traits;
            if (traits != null)
                traits.Color = value;
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            attached.Price.OnPostSet.Remove(_eventsGuid);
            attached.Moxie.OnPostSet.Remove(_eventsGuid);
            attached.Health.OnPostSet.Remove(_eventsGuid);
            attached.Strength.OnPostSet.Remove(_eventsGuid);
            attached.Traits.Passives.OnStacksChanged.Remove(_eventsGuid);
            attached.Traits.Actives.OnStacksChanged.Remove(_eventsGuid);
        }
        protected override int UpperLeftIconDisplayValue()
        {
            return attached.Price;
        }
        protected override int UpperRightIconDisplayValue()
        {
            return attached.Moxie;
        }
        protected override int LowerLeftIconDisplayValue()
        {
            return attached.Health;
        }
        protected override int LowerRightIconDisplayValue()
        {
            return attached.Strength;
        }

        protected override string UpperLeftIconTooltip()
        {
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);
            int priceDefault = attached.Data.price.value;
            return Translator.GetString("table_field_card_drawer_1", priceCurrencyStr, priceDefault, attached.Price.ToStringRich(priceDefault));
        }
        protected override string UpperRightIconTooltip()
        {
            int moxieDefault = attached.Data.moxie;
            int initiationOrder = (attached as BattleFieldCard)?.InitiationOrder ?? -1;
            if (initiationOrder == -1)
                 return Translator.GetString("table_field_card_drawer_2", moxieDefault, attached.Moxie.ToStringRich(moxieDefault));
            else return Translator.GetString("table_field_card_drawer_3", moxieDefault, attached.Moxie.ToStringRich(moxieDefault), initiationOrder);
        }
        protected override string LowerLeftIconTooltip()
        {
            int healthDefault = attached.Data.health;
            return Translator.GetString("table_field_card_drawer_4", healthDefault, attached.Health.ToStringRich(healthDefault));
        }
        protected override string LowerRightIconTooltip()
        {
            int strengthDefault = attached.Data.strength;
            return Translator.GetString("table_field_card_drawer_5", strengthDefault, attached.Strength.ToStringRich(strengthDefault));
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;

            TableTraitListSetDrawer setDrawer = Traits;
            if (setDrawer == null) return;
            if (setDrawer.queue.IsEmpty) return;
            if (queue.IsRunning) return;

            Traits?.ShowStoredElementsInstantly();
            if (!this.HasInitiationPreview())
                ShowBgInstantly();
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);

            TableTraitListSetDrawer setDrawer = Traits;
            if (this.HasInitiationPreview() || queue.IsRunning) return;
            if (setDrawer != null && setDrawer.queue.IsRunning) return;

            setDrawer?.HideStoredElementsInstantly();
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
    }
}
