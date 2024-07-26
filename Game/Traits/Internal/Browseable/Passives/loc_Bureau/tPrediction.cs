using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPrediction : PassiveTrait
    {
        const string ID = "prediction";
        const int PRIORITY = 4;
        const int MOXIE_PER_STACK = 1;

        public tPrediction() : base(ID)
        {
            name = "Предсказание";
            desc = "Я предсказываю, что ты прочтёшь этот текст. Скорее всего, ровно один раз.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf, PRIORITY);
        }
        protected tPrediction(tPrediction other) : base(other) { }
        public override object Clone() => new tPrediction(this);

        public override string DescRich(ITableTrait trait)
        {
            int effect = MOXIE_PER_STACK * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении любой союзной карты на территории (П{PRIORITY})",
                    $"увеличивает её инициативу на <u>{effect}</u>. Эффект пропадает в случае, если: заряды истощаются, карта перестаёт быть союзной или владелец погибает."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 40 * Mathf.Pow(stacks - 1, 2);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            if (e.Trait.WasAdded(e)) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;
            IEnumerable<BattleFieldCard> cards = trait.Area.PotentialTargets().WithCard().Select(f => f.Card);

            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.moxie.AdjustValue(e.delta, trait);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            BattlePassiveTrait trait = (BattlePassiveTrait)e.trait;
            if (trait == null) return;

            int moxie = MOXIE_PER_STACK * trait.GetStacks();
            string guid = trait.GuidGen(e.target.Guid);

            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await e.target.moxie.AdjustValue(moxie, trait, guid);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await e.target.moxie.RevertValue(guid);
            }
        }
    }
}
