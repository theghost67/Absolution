using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPlanned : ActiveTrait
    {
        const string ID = "planned";
        const int CD = 1;
        static readonly TraitStatFormula _statsF = new(true, 0.20f, 0.05f);

        public tPlanned() : base(ID)
        {
            name = Translator.GetString("trait_planned_1");
            desc = Translator.GetString("trait_planned_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tPlanned(tPlanned other) : base(other) { }
        public override object Clone() => new tPlanned(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_planned_3", _statsF.Format(args.stacks), CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 1, 1.5f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Card.Traits.Count != 0;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            int count = target.Traits.Count;
            float statsBonus = _statsF.Value(e.traitStacks);
            float totalBonus = count * statsBonus;
            await target.Traits.Passives.Clear(trait);
            if (!target.IsKilled)
                await target.Traits.Actives.Clear(trait);
            if (!target.IsKilled)
                await target.Health.AdjustValueScale(totalBonus, trait);
            if (!target.IsKilled)
                await target.Strength.AdjustValueScale(totalBonus, trait);
            trait.SetCooldown(0);
        }
    }
}
