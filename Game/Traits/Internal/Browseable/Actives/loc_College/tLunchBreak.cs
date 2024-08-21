using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLunchBreak : ActiveTrait
    {
        const string ID = "lunch_break";
        static readonly TraitStatFormula _healthF = new(false, 0, 2);
        const int CD = 1;

        public tLunchBreak() : base(ID)
        {
            name = "Перекус";
            desc = "Неплохо было бы подкрепиться.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tLunchBreak(tLunchBreak other) : base(other) { }
        public override object Clone() => new tLunchBreak(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string healthF = _healthF.Format(args.stacks, true);
            return $"<color>При использовании на карте рядом</color>\nЛечит цель на {healthF}. Перезарядка: {CD} х.\n\n" +
                   $"<color>При использовании на поле рядом</color>\nЛечит сторону, которая владеет этим полем, на {healthF}. Перезарядка: {CD} х.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(8, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;

            trait.SetCooldown(CD);
            int health = (int)_healthF.Value(e.traitStacks);

            if (target.Card != null)
                 await target.Card.Health.AdjustValue(health, trait);
            else await target.health.AdjustValue(health, trait);
        }
    }
}
