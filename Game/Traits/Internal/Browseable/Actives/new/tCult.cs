using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCult : ActiveTrait
    {
        const string ID = "cult";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);
        static readonly TraitStatFormula _statsF = new(true, 0.15f, 0.15f);
        static readonly TraitStatFormula _damageF = new(true, 0.00f, 1.00f);

        public tCult() : base(ID)
        {
            name = Translator.GetString("trait_cult_1");
            desc = Translator.GetString("trait_cult_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tCult(tCult other) : base(other) { }
        public override object Clone() => new tCult(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_cult_3", name, _moxieF.Format(args.stacks, true), _damageF.Format(args.stacks), name, _statsF.Format(args.stacks, true));

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(10, stacks, 1, 1.65f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.Owner.Field == null) return;

            float stats = _statsF.Value(e.delta.Abs());
            if (e.delta < 0)
                stats = -stats;
            await trait.Owner.Health.AdjustValueScale(stats, trait);
            if (trait.Owner.IsKilled) return;
            await trait.Owner.Strength.AdjustValueScale(stats, trait);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Card.Traits.Active(ID) == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            await target.Card.Traits.Actives.AdjustStacks(ID, e.traitStacks, trait);
            await target.Card.Moxie.AdjustValue(-_moxieF.ValueInt(e.traitStacks), trait);
            float damageScale = _damageF.Value(e.traitStacks);
            int damage = (int)Mathf.Ceil(damageScale * target.Card.Strength);
            await owner.Side.Health.AdjustValue(-damage, trait);
        }
    }
}
