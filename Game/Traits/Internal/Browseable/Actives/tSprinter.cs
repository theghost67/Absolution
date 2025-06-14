﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSprinter : ActiveTrait
    {
        const string ID = "sprinter";

        public tSprinter() : base(ID)
        {
            name = "Спринтер";
            desc = "Ну что, побегаем?";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tSprinter(tSprinter other) : base(other) { }
        public override object Clone() => new tSprinter(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на пустом союзном поле</color>\nПеремещает владельца на указанное поле. Тратит один заряд.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.1f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.8f);
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
            await owner.TryAttachToField(target, trait);
            await trait.AdjustStacks(-1, owner.Side);
        }
    }
}
