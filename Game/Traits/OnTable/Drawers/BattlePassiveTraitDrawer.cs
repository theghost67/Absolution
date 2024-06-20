using Game.Cards;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattlePassiveTrait"/>.
    /// </summary>
    public class BattlePassiveTraitDrawer : TablePassiveTraitDrawer
    {
        public readonly new BattlePassiveTrait attached;
        public BattlePassiveTraitDrawer(BattlePassiveTrait trait, Transform parent) : base(trait, parent)
        {
            attached = trait;
        }

        protected override void OnMouseEnterBase()
        {
            BattleFieldCard owner = attached.Owner;
            if (owner == null) return;
            if (owner.Field != null)
                attached.Area.CreateTargetsHighlight();
        }
        protected override void OnMouseLeaveBase()
        {
            BattleFieldCard owner = attached.Owner;
            if (owner == null) return;
            if (owner.Field != null)
                attached.Area.DestroyTargetsHighlight();
        }
    }
}
