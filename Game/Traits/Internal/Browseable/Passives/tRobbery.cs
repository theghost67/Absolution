using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tRobbery : PassiveTrait
    {
        const string ID = "robbery";
        static readonly TraitStatFormula _goldF = new(false, 0, 1);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tRobbery() : base(ID)
        {
            name = Translator.GetString("trait_robbery_1");
            desc = "Guys, the drill, go get it.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tRobbery(tRobbery other) : base(other) { }
        public override object Clone() => new tRobbery(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_robbery_2", _goldF.Format(args.stacks), _moxieF.Format(args.stacks, true));

        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await owner.Side.Gold.AdjustValue(_goldF.Value(stacks), trait);
            await owner.Moxie.AdjustValue(-_moxieF.Value(stacks), trait);
        }
    }
}
