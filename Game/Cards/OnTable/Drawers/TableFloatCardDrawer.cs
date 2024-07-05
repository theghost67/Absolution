using MyBox;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableFloatCard"/>.
    /// </summary>
    public class TableFloatCardDrawer : TableCardDrawer
    {
        public readonly new TableFloatCard attached;
        public TableFloatCardDrawer(TableFloatCard card, Transform parent) : base(card, parent, redrawIcons: false)
        {
            attached = card;
            RedrawIcons();

            moxieIcon.RedrawValueAsNull();
            healthIcon.RedrawValueAsNull();
            strengthIcon.RedrawValueAsNull();
            RedrawSubheader("Способность");
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
            Tooltip.Show($"Отсутствует у карт способностей\n<color=grey><i>Инициатива: определяет быстроту действий.");
        }
        protected override void OnLowerLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            Tooltip.Show($"Отсутствует у карт способностей\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.");
        }
        protected override void OnLowerRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            Tooltip.Show($"Отсутствует у карт способностей\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.");
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
    }
}
