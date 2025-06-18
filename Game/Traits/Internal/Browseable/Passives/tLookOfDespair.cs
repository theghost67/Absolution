using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLookOfDespair : PassiveTrait
    {
        const string ID = "look_of_despair";
        static readonly TraitStatFormula _moxieF = new(false, 5, 0);

        public tLookOfDespair() : base(ID)
        {
            name = Translator.GetString("trait_look_of_despair_1");
            desc = Translator.GetString("trait_look_of_despair_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tLookOfDespair(tLookOfDespair other) : base(other) { }
        public override object Clone() => new tLookOfDespair(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_look_of_despair_3", _moxieF.Format(args.stacks, true));
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            IBattleTrait trait = e.trait;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await e.target.Moxie.AdjustValue(-_moxieF.Value(e.traitStacks), trait, trait.GuidStr);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await e.target.Moxie.RevertValue(trait.GuidStr);
            }
        }
    }
}
