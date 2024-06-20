using Game.Cards;
using Game.Effects;
using Game.Sleeves;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattleActiveTraitListElement"/>.
    /// </summary>
    public class BattleActiveTraitListElementDrawer : TableActiveTraitListElementDrawer
    {
        public readonly new BattleActiveTraitListElement attached;
        readonly BattleActiveTrait _attachedTrait;

        public BattleActiveTraitListElementDrawer(BattleActiveTraitListElement element, Transform parent) : base(element, parent) 
        {
            attached = element;
            _attachedTrait = attached.Trait;
            ChangePointer = _attachedTrait.Side.isMe;
        }

        protected override bool ShakeOnMouseClickLeft() => false;

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;

            BattleFieldCard owner = _attachedTrait.Owner;
            if (owner == null) return;
            if (owner.Field != null)
                _attachedTrait.Area.CreateTargetsHighlight();
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            if (e.handled) return;

            BattleFieldCard owner = _attachedTrait.Owner;
            if (owner == null) return;
            if (owner.Field != null)
                _attachedTrait.Area.DestroyTargetsHighlight();
        }
        protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseClickLeftBase(sender, e);
            if (e.handled) return;

            bool used = _attachedTrait.TryUseWithAim(attached.Trait.Territory.player);
            if (!used)
                transform.DOAShake();
            else if (_attachedTrait.Owner is ITableSleeveCard sleeveCard)
                sleeveCard.Sleeve.Drawer.PullIn();
        }
    }
}
