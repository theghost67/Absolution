﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tEmpoweringBeam : ActiveTrait
    {
        const string ID = "empowering_beam";
        const int CD = 1;
        const string OTHER_BEAM_ID = "healing_beam";
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tEmpoweringBeam() : base(ID)
        {
            name = "Усиливающий луч";
            desc = "Покажи им!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tEmpoweringBeam(tEmpoweringBeam other) : base(other) { }
        public override object Clone() => new tEmpoweringBeam(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на карте рядом",
                    $"Увеличивает силу цели на {_strengthF.Format(trait)}. Все лучи уходят на перезарядку: {CD} х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(10, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(_strengthF.Value(result.Entity));
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

            trait1.Storage.turnsDelay += CD;
            if (trait2 != null)
                trait2.Storage.turnsDelay += CD;

            await card.Strength.AdjustValue(_strengthF.Value(trait1), trait1);
        }
    }
}