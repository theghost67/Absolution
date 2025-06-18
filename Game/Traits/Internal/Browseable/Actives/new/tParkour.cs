using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tParkour : ActiveTrait
    {
        const string ID = "parkour";
        const int CD = 1;
        static readonly TraitStatFormula _healthF = new(false, 0, 1);
        static readonly TraitStatFormula _strengthF = new(false, 0, 1);

        public tParkour() : base(ID)
        {
            name = Translator.GetString("trait_parkour_1");
            desc = "This. Is. Parkour.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tParkour(tParkour other) : base(other) { }
        public override object Clone() => new tParkour(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_parkour_2", _healthF.Format(args.stacks), _strengthF.Format(args.stacks), CD);

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(4, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            int stacks = e.traitStacks;

            trait.SetCooldown(CD);
            await owner.TryAttachToField(target, trait);
            if (owner.IsKilled) return;

            int health = _healthF.ValueInt(stacks);
            int strength = _strengthF.ValueInt(stacks);
            await owner.Health.AdjustValue(strength, trait);
            if (owner.IsKilled) return;
            await owner.Strength.AdjustValue(strength, trait);
        }
    }
}
