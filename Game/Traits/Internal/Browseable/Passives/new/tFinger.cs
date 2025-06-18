using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFinger : PassiveTrait
    {
        const string ID = "finger";
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.25f);
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tFinger() : base(ID)
        {
            name = Translator.GetString("trait_finger_1");
            desc = Translator.GetString("trait_finger_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tFinger(tFinger other) : base(other) { }
        public override object Clone() => new tFinger(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_finger_3", _moxieF.Format(args.stacks, true), _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks);
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            IBattleTrait trait = e.trait;
            string entryId = $"{trait.Guid}/{e.target.Guid}";

            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await e.target.Moxie.AdjustValue(-_moxieF.ValueInt(e.traitStacks), trait, entryId);
                await e.target.Strength.AdjustValueScale(-_strengthF.Value(e.traitStacks), trait, entryId);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await e.target.Moxie.RevertValue(entryId);
                await e.target.Strength.RevertValueScale(entryId);
            }
        }
    }
}
