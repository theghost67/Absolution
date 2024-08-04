using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTeleportationScroll : ActiveTrait
    {
        const string ID = "teleportation_scroll";

        public tTeleportationScroll() : base(ID)
        {
            name = "Свиток телепортации";
            desc = "ТП на мид.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tTeleportationScroll(tTeleportationScroll other) : base(other) { }
        public override object Clone() => new tTeleportationScroll(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на пустом союзном поле",
                    $"Перемещает владельца на указанное поле. Тратит один заряд."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
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
            BattleFieldCard owner = trait.Owner;
            await owner.TryAttachToField(target, trait);
            await trait.AdjustStacks(-1, owner.Side);
        }
    }
}
