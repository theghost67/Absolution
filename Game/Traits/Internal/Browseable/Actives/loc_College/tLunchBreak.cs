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
        static readonly TraitStatFormula _healthF = new(false, 0, 1);
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

        public override string DescRich(ITableTrait trait)
        {
            string healthF = _healthF.Format(trait);
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на карте рядом",
                    $"Лечит цель на <u>{healthF}</u> ед. Перезарядка: {CD} х."),
                new($"При использовании на территории на поле рядом",
                    $"Лечит сторону, которая владеет этим полем, на <u>{healthF}</u> ед. Перезарядка: {CD} х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(6, stacks);
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

            trait.Storage.turnsDelay += CD;
            int health = (int)_healthF.Value(trait);

            if (target.Card != null)
                 await target.Card.Health.AdjustValue(health, trait);
            else await target.health.AdjustValue(health, trait);
        }
    }
}
