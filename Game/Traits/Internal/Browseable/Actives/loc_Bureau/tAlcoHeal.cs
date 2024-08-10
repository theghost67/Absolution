using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAlcoHeal : ActiveTrait
    {
        const string ID = "alco_heal";
        static readonly TraitStatFormula _healthIncF = new(false, 0, 3);
        static readonly TraitStatFormula _moxieDecF = new(false, 2, 0);
        const int CD = 1;

        public tAlcoHeal() : base(ID)
        {
            name = "Алкогольная подпитка";
            desc = "";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tAlcoHeal(tAlcoHeal other) : base(other) { }
        public override object Clone() => new tAlcoHeal(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на карте рядом",
                    $"Восстанавливает {_healthIncF.Format(trait)} здоровья цели, уменьшает её инициативу на {_moxieDecF.Format(trait)}. Перезарядка: {CD} х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(8, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(_healthIncF.Value(result.Entity));
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard card = (BattleFieldCard)e.target.Card;

            int stacks = trait.GetStacks();
            trait.Storage.turnsDelay += CD;
            await card.Health.AdjustValue(_healthIncF.Value(trait, stacks), trait);
            await card.Moxie.AdjustValue(-_moxieDecF.Value(trait, stacks), trait);
        }
    }
}
