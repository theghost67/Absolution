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

        protected override string UpperLeftIconTooltip()
        {
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);

            int priceDefault = attached.Data.price.value;
            int priceCurrent = attached.price;
            return $"Валюта: {priceCurrencyStr}\nПо умолчанию: {priceDefault} ед.\nТекущее: {priceCurrent} ед.\n<color=grey><i>Стоимость: цена установки на территорию.";
        }
        protected override string UpperRightIconTooltip()
        {
            return $"Отсутствует у карт способностей\n<color=grey><i>Инициатива: определяет быстроту действий.";
        }
        protected override string LowerLeftIconTooltip()
        {
            return $"Отсутствует у карт способностей\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.";
        }
        protected override string LowerRightIconTooltip()
        {
            return $"Отсутствует у карт способностей\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.";
        }
    }
}
