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
        const int MOXIE_ABS_INCREASE_ON_OWNER_SIDE_SEEN = 1;

        public tBecomeHuman() : base(ID)
        {
            name = "Стать человеком";
            desc = "- Ты просто долбанная машина!\n- Разумеется, Лейтенант, я машина. А вы ожидали чего-то другого?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf, PRIORITY);
        }
        protected tBecomeHuman(tBecomeHuman other) : base(other) { }
        public override object Clone() => new tBecomeHuman(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(OBS_CARD_ID).name;
            float strengthEffect = STRENGTH_REL_INCREASE_ON_OWNER_SIDE_SEEN * 100;
            int moxieEffect = MOXIE_ABS_INCREASE_ON_OWNER_SIDE_SEEN;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты <i>{cardName}</i> на союзной стороне (П{PRIORITY})",
                    $"Увеличивает свою силу на {strengthEffect}% и инициативу на {moxieEffect} ед."),
            });
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            string guid = trait.GuidGen(e.target.Guid);

            if (e.target.Data.id != OBS_CARD_ID) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await owner.strength.AdjustValueScale(STRENGTH_REL_INCREASE_ON_OWNER_SIDE_SEEN, trait, guid);
                await owner.moxie.AdjustValue(MOXIE_ABS_INCREASE_ON_OWNER_SIDE_SEEN, trait, guid);
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
