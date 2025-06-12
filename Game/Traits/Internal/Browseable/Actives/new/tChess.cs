using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tChess : ActiveTrait
    {
        const string ID = "chess";
        const int CD = 1;
        static readonly TraitStatFormula _strengthF = new(true, 0.20f, 0.05f);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tChess() : base(ID)
        {
            name = "Сыграем?";
            desc = "Миш, у меня для тебя одно важное признание. В отличие от тебя трёхлетнего, я играть не умею. Поэтому мы тебе нашли достойного противника.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tChess(tChess other) : base(other) { }
        public override object Clone() => new tChess(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на незанятом союзном поле</color>\nПеремещает владельца на указанное поле. " +
                   $"После успешного перемещения, владелец получит {_strengthF.Format(args.stacks)} силы и понизит свою инициативу на {_moxieF.Format(args.stacks, true)}. Перезарядка: {CD} х.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _strengthF.Value(result.Entity.GetStacks()));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks);
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

            float strength = _strengthF.Value(stacks);
            int moxie = _moxieF.ValueInt(stacks);
            await owner.Strength.AdjustValueScale(strength, trait);

            if (owner.IsKilled) return;
            await owner.Moxie.AdjustValue(-moxie, trait);
        }
    }
}
