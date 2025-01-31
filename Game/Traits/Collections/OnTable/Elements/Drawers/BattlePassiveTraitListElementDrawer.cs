﻿using Game.Cards;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattlePassiveTraitListElement"/>.
    /// </summary>
    public class BattlePassiveTraitListElementDrawer : TablePassiveTraitListElementDrawer
    {
        public readonly new BattlePassiveTraitListElement attached;
        readonly BattlePassiveTrait _attachedTrait;

        public BattlePassiveTraitListElementDrawer(BattlePassiveTraitListElement element, Transform parent) : base(element, parent) 
        {
            attached = element;
            _attachedTrait = attached.Trait;
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            BattleFieldCard owner = _attachedTrait.Owner;
            if (owner == null) return;
            _attachedTrait.Area.CreateTargetsHighlight();
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            BattleFieldCard owner = _attachedTrait.Owner;
            if (owner == null) return;
            _attachedTrait.Area.DestroyTargetsHighlight();
        }
    }
}
