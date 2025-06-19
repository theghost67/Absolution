using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAlcoRage : PassiveTrait
    {
        const string ID = "alco_rage";
        static readonly TraitStatFormula _healthF = new(true, 0.15f, 0.15f);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tAlcoRage() : base(ID)
        {
            name = Translator.GetString("trait_alco_rage_1");
            desc = Translator.GetString("trait_alco_rage_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tAlcoRage(tAlcoRage other) : base(other) { }
        public override object Clone() => new tAlcoRage(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_alco_rage_3", _healthF.Format(args.stacks), _moxieF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 1, 1.6f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.victim.Side == owner.Side) return;

            int stacks = trait.GetStacks();
            float health = owner.Data.health * _healthF.Value(stacks);
            float moxie = -_moxieF.Value(stacks);

            await trait.AnimActivation();
            await owner.Health.AdjustValue(health, trait);
            await owner.Moxie.AdjustValue(moxie, trait);
        }
    }
}
