using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDontStarve : PassiveTrait
    {
        const string ID = "dont_starve";
        static readonly TraitStatFormula _healthF = new(true, 0.15f, 0.05f);

        public tDontStarve() : base(ID)
        {
            name = Translator.GetString("trait_dont_starve_1");
            desc = Translator.GetString("trait_dont_starve_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tDontStarve(tDontStarve other) : base(other) { }
        public override object Clone() => new tDontStarve(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_dont_starve_3", _healthF.Format(args.stacks));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 1, 1.65f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }
        static async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] emptyFields = trait.Side.Fields().WithoutCard().Select(f => f.Card).ToArray();
            if (emptyFields.Length == 0) return;

            float health = emptyFields.Length * _healthF.Value(trait.GetStacks());

            await trait.AnimActivation();
            await owner.Health.AdjustValueScale(health, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
