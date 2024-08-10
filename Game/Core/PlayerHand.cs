﻿using DG.Tweening;
using Game.Sleeves;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий руку игрока для карт рукава, где игрок сам решает, когда возвращать и бросать карту.
    /// </summary>
    public static class PlayerHand
    {
        static bool _isHoldingAnyCard;
        static bool _isInCooldown;
        static ITableSleeveCard _card;

        static PlayerHand() { Global.OnUpdate += OnUpdate; }

        public static void TryTakeCard(ITableSleeveCard card)
        {
            if (_isInCooldown || _isHoldingAnyCard || !card.TryTake())
                return;

            _card = card;
            _isHoldingAnyCard = true;

            _isInCooldown = true;
            DOVirtual.DelayedCall(0.12f, () => _isInCooldown = false);
        }
        static void TryReturnCard()
        {
            if (_isInCooldown || !_isHoldingAnyCard || !_card.TryReturn())
                return;

            _card = null;
            _isHoldingAnyCard = false;
        }
        static void TryDropCard()
        {
            TableFieldDrawer drawer = (TableFieldDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableFieldDrawer);
            TableField field = drawer?.attached;

            if (field == null || _isInCooldown || !_isHoldingAnyCard)
                return;
            if (TableEventManager.CanAwaitAnyEvents() || !_card.TryDropOn(field))
                return;

            _card = null;
            _isHoldingAnyCard = false;
        }

        static void OnUpdate()
        {
            if (!_isHoldingAnyCard) return;

            if (Input.GetMouseButtonDown(0))
                TryDropCard();
            else if (Input.GetMouseButtonDown(1))
                TryReturnCard();
        }
    }
}
