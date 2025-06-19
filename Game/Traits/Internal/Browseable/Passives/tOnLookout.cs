using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOnLookout : PassiveTrait
    {
        const string ID = "on_lookout";
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tOnLookout() : base(ID)
        {
            name = Translator.GetString("trait_on_lookout_1");
            desc = Translator.GetString("trait_on_lookout_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tOnLookout(tOnLookout other) : base(other) { }
        public override object Clone() => new tOnLookout(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_on_lookout_3", _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            if (!e.canSeeTarget) return;

            await trait.AnimDetectionOnSeen(e.target);
            int strength = _strengthF.ValueInt(e.traitStacks);
            BattleInitiationSendArgs initiation = new(trait.Owner, strength, true, false, e.target.Field);
            await trait.Territory.Initiations.EnqueueAndAwait(initiation);
        }        
    }
}
