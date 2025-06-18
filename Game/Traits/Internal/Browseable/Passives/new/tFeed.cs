using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFeed : PassiveTrait
    {
        const string ID = "feed";
        static readonly TraitStatFormula _strengthF = new(false, 0, 1);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tFeed() : base(ID)
        {
            name = Translator.GetString("trait_feed_1");
            desc = Translator.GetString("trait_feed_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tFeed(tFeed other) : base(other) { }
        public override object Clone() => new tFeed(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_feed_3", _moxieF.Format(args.stacks), _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }
        static async UniTask OnKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            int moxie = _moxieF.ValueInt(trait.GetStacks());
            int strength = _strengthF.ValueInt(trait.GetStacks());

            await trait.AnimActivation();
            await owner.Moxie.AdjustValue(-moxie, trait);
            owner.Data.strength += strength;
        }
    }
}
