using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattleFieldCard"/>.
    /// </summary>
    public class BattleFieldCardDrawer : TableFieldCardDrawer
    {
        public readonly new BattleFieldCard attached;
        public BattleFieldCardDrawer(BattleFieldCard card, Transform parent) : base(card, parent) 
        {
            attached = card;
        }

        protected override bool RedrawRangeFlipY() => attached.Side.isMe;

        protected override void OnUpperLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            int priceDefault = attached.Data.moxie;
            Game.Tooltip.Show($"По умолчанию: {priceDefault}.\nТекущее: {attached.price.StatToStringRich(priceDefault)} ед.\n<color=grey><i>Стоимость: цена установки на территорию.");
        }
        protected override void OnUpperRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int moxieDefault = attached.Data.moxie;
            int initiationOrder = attached.InitiationOrder;
            Game.Tooltip.Show($"По умолчанию: {moxieDefault}.\nТекущее: {attached.moxie.StatToStringRich(moxieDefault)} ед.\nПозиция в очереди: {initiationOrder}.\n<color=grey><i>Инициатива: определяет быстроту действий.");
        }
        protected override void OnLowerLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int healthDefault = attached.Data.health;
            Game.Tooltip.Show($"По умолчанию: {healthDefault} ед.\nТекущее: {attached.health.StatToStringRich(healthDefault)} ед.\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.");
        }
        protected override void OnLowerRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int strengthDefault = attached.Data.strength;
            Game.Tooltip.Show($"По умолчанию: {strengthDefault} ед.\nТекущее: {attached.strength.StatToStringRich(strengthDefault)} ед.\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.");
        }
    }
}
