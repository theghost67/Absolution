using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFoodPoisoning : PassiveTrait
    {
        const string ID = "food_poisoning";
        static readonly TraitStatFormula _healthF = new(true, 0.50f, 0.50f);

        public tFoodPoisoning() : base(ID)
        {
            name = Translator.GetString("trait_food_poisoning_1");
            desc = Translator.GetString("trait_food_poisoning_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tFoodPoisoning(tFoodPoisoning other) : base(other) { }
        public override object Clone() => new tFoodPoisoning(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_food_poisoning_3", _healthF.Format(args.stacks, true));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.65f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null) return;

            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (killer == null) return;

            float value = -_healthF.Value(trait.GetStacks());
            await trait.AnimActivation();
            await killer.Strength.AdjustValueScale(value, trait);
            await killer.Health.AdjustValueScale(value, trait);
        }
    }
}
