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
    public class tNerfTime : ActiveTrait
    {
        const string ID = "nerf_time";
        static readonly TraitStatFormula _enemyStatsF = new(true, 0.25f, 0.00f);
        static readonly TraitStatFormula _enemyMoxieF = new(false, 1, 0);
        static readonly TraitStatFormula _allyMoxieF = new(false, 1, 0);

        public tNerfTime() : base(ID)
        {
            name = "Время нёрфа";
            desc = "Нам кажется, что этот персонаж слишком силён. Мы удаляем все навыки данного персонажа.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tNerfTime(tNerfTime other) : base(other) { }
        public override object Clone() => new tNerfTime(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на любой вражеской карте</color>\n" +
                   $"Удаляет все навыки у цели, а также понижает её инициативу на {_enemyMoxieF.Format(args.stacks)} и характеристики на {_enemyStatsF.Format(args.stacks)}, " +
                   $"понижает инициативу всех союзных карт, кроме себя, на {_allyMoxieF.Format(args.stacks, true)}. Тратит один заряд.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(16, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, TerritoryRange.ownerAllNotSelf).WithCard();

            target.Traits.Clear(trait);

            int enemyMoxie = _enemyMoxieF.ValueInt(e.traitStacks);
            float enemyStats = _enemyStatsF.Value(e.traitStacks);
            await target.Moxie.AdjustValue(enemyMoxie, trait);
            await target.Health.AdjustValueScale(-enemyStats, trait);
            await target.Strength.AdjustValueScale(-enemyStats, trait);

            int allyMoxie = _allyMoxieF.ValueInt(e.traitStacks);
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
                await card.Moxie.AdjustValue(-allyMoxie, trait);
            await trait.AdjustStacks(-1, owner.Side);
        }
    }
}
