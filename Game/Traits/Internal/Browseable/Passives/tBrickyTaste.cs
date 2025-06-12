using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBrickyTaste : PassiveTrait
    {
        const string ID = "bricky_taste";
        static readonly TraitStatFormula _moxieF = new(false, 0, 1);

        public tBrickyTaste() : base(ID)
        {
            name = "Кирпичный привкус";
            desc = "Ты сейчас кирпич зубами грызть будешь.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tBrickyTaste(tBrickyTaste other) : base(other) { }
        public override object Clone() => new tBrickyTaste(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После совершения атаки или лечения владельца</color>\nУменьшает инициативу атакующего на {_moxieF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(24, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivationShort();
            await e.Sender.Moxie.AdjustValue(-_moxieF.Value(trait.GetStacks()), trait);
        }
    }
}
