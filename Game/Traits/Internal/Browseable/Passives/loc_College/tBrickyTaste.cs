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
        const int PRIORITY = 2;
        static readonly TraitStatFormula _moxieF = new(false, 0, 2);

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
            return $"<color>После атаки/лечения на владельца (П{PRIORITY})</color>\nУменьшает инициативу атакующего на {_moxieF.Format(args.stacks, true)}.";
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
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await e.Sender.Moxie.AdjustValue(-_moxieF.Value(trait.GetStacks()), trait);
        }
    }
}
