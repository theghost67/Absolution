﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHealingBeam : ActiveTrait
    {
        const string ID = "healing_beam";
        const int CD = 1;
        const string OTHER_BEAM_ID = "empowering_beam";
        static readonly TraitStatFormula _healthF = new(false, 0, 4);

        public tHealingBeam() : base(ID)
        {
            name = "Лечебный луч";
            desc = "Я с тобой.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tHealingBeam(tHealingBeam other) : base(other) { }
        public override object Clone() => new tHealingBeam(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При использовании на союзной карте рядом</color>\nУвеличивает здоровье цели на {_healthF.Format(args.stacks, true)}. Все лучи уходят на перезарядку: {CD} х.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(12, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(_healthF.Value(result.Entity.GetStacks()) * 1.2f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait1 = (IBattleTrait)e.trait;
            IBattleTrait trait2 = trait1.Owner.Traits.Any(OTHER_BEAM_ID);
            BattleFieldCard card = (BattleFieldCard)e.target.Card;

            trait1.SetCooldown(CD);
            trait2.SetCooldown(CD);

            await card.Health.AdjustValue(_healthF.Value(e.traitStacks), trait1);
        }
    }
}
