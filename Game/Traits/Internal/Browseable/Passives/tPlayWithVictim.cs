using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPlayWithVictim : PassiveTrait
    {
        const string ID = "play_with_victim";
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.3333f);

        public tPlayWithVictim() : base(ID)
        {
            name = Translator.GetString("trait_play_with_victim_1");
            desc = Translator.GetString("trait_play_with_victim_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tPlayWithVictim(tPlayWithVictim other) : base(other) { }
        public override object Clone() => new tPlayWithVictim(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_play_with_victim_3", _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(10, stacks, 1, 1.8f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.ReceiverCard == null || e.ReceiverCard.IsKilled) return;

            await trait.AnimActivationShort();
            await e.ReceiverCard.Strength.AdjustValueScale(-_strengthF.Value(trait.GetStacks()), trait);
        }
    }
}
