using Game.Cards;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="BattleActiveTrait"/>.
    /// </summary>
    public class BattleActiveTraitDrawer : TableActiveTraitDrawer
    {
        public readonly new BattleActiveTrait attached;
        public BattleActiveTraitDrawer(BattleActiveTrait trait, Transform parent) : base(trait, parent)
        {
            attached = trait;
        }

        protected override bool ChangePointerBase()
        {
            return attached.Side.isMe;
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
