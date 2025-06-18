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
        public TableFloatCardDrawer(TableFloatCard card, Transform parent) : base(card, parent)
        {
            attached = card;

            RedrawSubheader(Translator.GetString("table_float_card_drawer_1"));
            RedrawIcons();
            RedrawOutlineInstantly();

            moxieIcon.RedrawValueAsNull();
            healthIcon.RedrawValueAsNull();
            strengthIcon.RedrawValueAsNull();
        }

        protected override string UpperLeftIconTooltip()
        {
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);

            int priceDefault = attached.Data.price.value;
            int priceCurrent = attached.Price;
            return Translator.GetString("table_float_card_drawer_2", priceCurrencyStr, priceDefault, priceCurrent);
        }
        protected override string UpperRightIconTooltip()
        {
            return Translator.GetString("table_float_card_drawer_3");
        }
        protected override string LowerLeftIconTooltip()
        {
            return Translator.GetString("table_float_card_drawer_4");
        }
        protected override string LowerRightIconTooltip()
        {
            return Translator.GetString("table_float_card_drawer_5");
        }
    }
}
