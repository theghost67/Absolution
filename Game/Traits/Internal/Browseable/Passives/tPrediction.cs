using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPrediction : PassiveTrait
    {
        const string ID = "prediction";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tPrediction() : base(ID)
        {
            name = Translator.GetString("trait_prediction_1");
            desc = Translator.GetString("trait_prediction_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tPrediction(tPrediction other) : base(other) { }
        public override object Clone() => new tPrediction(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_prediction_3", _moxieF.Format(args.stacks, true));

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
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

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
