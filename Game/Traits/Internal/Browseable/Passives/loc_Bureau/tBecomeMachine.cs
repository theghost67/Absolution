using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBecomeMachine : PassiveTrait
    {
        const string ID = "become_machine";
        const int PRIORITY = 4;
        const string OBS_CARD_ID = "connor";
        const float STRENGTH_REL_DECREASE_ON_OPPOSITE_SIDE_SEEN = 0.50f;
        const int MOXIE_ABS_DECREASE_ON_OPPOSITE_SIDE_SEEN = 1;

        public tBecomeMachine() : base(ID)
        {
            name = "Стать машиной";
            desc = "Я думал- Я думал, что...";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll, PRIORITY);
        }
        protected tBecomeMachine(tBecomeMachine other) : base(other) { }
        public override object Clone() => new tBecomeMachine(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(OBS_CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты <i>{cardName}</i> на вражеской стороне (П{PRIORITY})",
                    $"Уменьшает свою силу на <u>{STRENGTH_REL_DECREASE_ON_OPPOSITE_SIDE_SEEN * 100}%</u> и инициативу на <u>{MOXIE_ABS_DECREASE_ON_OPPOSITE_SIDE_SEEN}</u> ед."),
            });
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            BattlePassiveTrait trait = (BattlePassiveTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            string guid = trait.GuidGen(e.target.Guid);

            if (e.target.Data.id != OBS_CARD_ID) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await owner.strength.AdjustValueScale(-STRENGTH_REL_DECREASE_ON_OPPOSITE_SIDE_SEEN, trait, guid);
                await owner.moxie.AdjustValue(-MOXIE_ABS_DECREASE_ON_OPPOSITE_SIDE_SEEN, trait, guid);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await owner.strength.RevertValueScale(guid);
                await owner.moxie.RevertValue(guid);
            }
        }
    }
}
