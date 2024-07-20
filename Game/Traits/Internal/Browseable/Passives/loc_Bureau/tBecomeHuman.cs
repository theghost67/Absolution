using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBecomeHuman : PassiveTrait
    {
        const string ID = "become_human";
        const int PRIORITY = 4;
        const string OBS_CARD_ID = "connor";
        const float STRENGTH_REL_INCREASE_ON_OWNER_SIDE_SEEN = 0.25f;
        const float STRENGTH_REL_DECREASE_ON_OPPOSITE_SIDE_SEEN = 0.50f;
        const int MOXIE_ABS_INCREASE_ON_OWNER_SIDE_SEEN = 1;
        const int MOXIE_ABS_DECREASE_ON_OPPOSITE_SIDE_SEEN = 1;

        public tBecomeHuman() : base(ID)
        {
            name = "Стать человеком";
            desc = "- Ты просто долбанная машина!\n- Разумеется, Лейтенант, я машина. А вы ожидали чего-то другого?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.allNotSelf, PRIORITY);
        }
        protected tBecomeHuman(tBecomeHuman other) : base(other) { }
        public override object Clone() => new tBecomeHuman(this);

        public override string DescRich(ITableTrait trait)
        {
            int stacks = trait.GetStacks();
            string cardName = CardBrowser.GetCard(OBS_CARD_ID).name;
            float strengthRelIncreaseOnOwnerSideSeen = STRENGTH_REL_INCREASE_ON_OWNER_SIDE_SEEN * stacks;
            float strengthRelDecreaseOnOppositeSideSeen = STRENGTH_REL_DECREASE_ON_OPPOSITE_SIDE_SEEN * stacks;
            float moxieAbsIncreaseOnOwnerSideSeen = MOXIE_ABS_INCREASE_ON_OWNER_SIDE_SEEN * stacks;
            float moxieAbsDecreaseOnOppositeSideSeen = MOXIE_ABS_DECREASE_ON_OPPOSITE_SIDE_SEEN * stacks;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты <i>{cardName}</i> на союзной стороне (П{PRIORITY})",
                    $"Увеличивает свою силу на <u>{strengthRelIncreaseOnOwnerSideSeen}%</u> и инициативу на <u>{moxieAbsIncreaseOnOwnerSideSeen}</u> ед."),
                new($"При появлении карты <i>{cardName}</i> на вражеской стороне (П{PRIORITY})",
                    $"Уменьшает свою силу на <u>{strengthRelDecreaseOnOppositeSideSeen}%</u> и инициативу на <u>{moxieAbsDecreaseOnOppositeSideSeen}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 30 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (e.target.Data.id != OBS_CARD_ID) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            int stacks = trait.GetStacks();

            float strengthRelIncreaseOnOwnerSideSeen = STRENGTH_REL_INCREASE_ON_OWNER_SIDE_SEEN * stacks;
            float strengthRelDecreaseOnOppositeSideSeen = STRENGTH_REL_DECREASE_ON_OPPOSITE_SIDE_SEEN * stacks;
            float moxieAbsIncreaseOnOwnerSideSeen = MOXIE_ABS_INCREASE_ON_OWNER_SIDE_SEEN * stacks;
            float moxieAbsDecreaseOnOppositeSideSeen = MOXIE_ABS_DECREASE_ON_OPPOSITE_SIDE_SEEN * stacks;

            await trait.AnimActivation();
            if (e.canSeeTarget)
            {
                if (e.target.Side == owner.Side)
                {
                    await owner.strength.AdjustValueScale(strengthRelIncreaseOnOwnerSideSeen, trait);
                    await owner.moxie.AdjustValue(moxieAbsIncreaseOnOwnerSideSeen, trait);
                }   
                else
                {
                    await owner.strength.AdjustValueScale(strengthRelDecreaseOnOppositeSideSeen, trait);
                    await owner.moxie.AdjustValue(moxieAbsDecreaseOnOppositeSideSeen, trait);
                }
            }
            else
            {
                if (e.target.Side == owner.Side)
                {
                    await owner.strength.AdjustValueScale(-strengthRelIncreaseOnOwnerSideSeen, trait);
                    await owner.moxie.AdjustValue(-moxieAbsIncreaseOnOwnerSideSeen, trait);
                }
                else
                {
                    await owner.strength.AdjustValueScale(-strengthRelDecreaseOnOppositeSideSeen, trait);
                    await owner.moxie.AdjustValue(-moxieAbsDecreaseOnOppositeSideSeen, trait);
                }
            }
        }
    }
}
