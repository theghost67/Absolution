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
    public class tNerfTime : ActiveTrait
    {
        const string ID = "nerf_time";
        const int CD = 3;
        static readonly TraitStatFormula _enemyStatsF = new(true, 0.25f, 0.00f);
        static readonly TraitStatFormula _enemyMoxieF = new(false, 1, 0);
        static readonly TraitStatFormula _allyMoxieF = new(false, 1, 0);

        public tNerfTime() : base(ID)
        {
            name = Translator.GetString("trait_nerf_time_1");
            desc = Translator.GetString("trait_nerf_time_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tNerfTime(tNerfTime other) : base(other) { }
        public override object Clone() => new tNerfTime(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_nerf_time_3", _enemyMoxieF.Format(args.stacks), _enemyStatsF.Format(args.stacks), _allyMoxieF.Format(args.stacks, true), CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, TerritoryRange.ownerAllNotSelf).WithCard();

            target.Traits.Clear(trait);

            int enemyMoxie = _enemyMoxieF.ValueInt(e.traitStacks);
            float enemyStats = _enemyStatsF.Value(e.traitStacks);
            await target.Moxie.AdjustValue(enemyMoxie, trait);
            await target.Health.AdjustValueScale(-enemyStats, trait);
            await target.Strength.AdjustValueScale(-enemyStats, trait);

            int allyMoxie = _allyMoxieF.ValueInt(e.traitStacks);
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
                await card.Moxie.AdjustValue(-allyMoxie, trait);
            trait.SetCooldown(CD);
        }
    }
}
