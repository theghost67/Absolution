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
        static readonly TraitStatFormula _moxieF = new(false, 0, 1);

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

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При появлении любой союзной карты на территории (П{PRIORITY})</color>\n" +
                   $"увеличивает инициативу цели на {_moxieF.Format(args.stacks, true)}. Эффект пропадает в случае, если: заряды истощаются, карта перестаёт быть союзной или владелец погибает.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(24, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            if (e.trait.WasAdded(e)) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            IEnumerable<BattleFieldCard> cards = trait.Area.PotentialTargets().WithCard().Select(f => f.Card);

            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.Moxie.AdjustValue(e.delta, trait);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            if (trait == null) return;

            string guid = trait.GuidGen(e.target.Guid);
            if (e.canSeeTarget)
            {
                int moxie = (int)_moxieF.Value(e.traitStacks);
                await trait.AnimDetectionOnSeen(e.target);
                await e.target.Moxie.AdjustValue(moxie, trait, guid);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await e.target.Moxie.RevertValue(guid);
            }
        }
    }
}
