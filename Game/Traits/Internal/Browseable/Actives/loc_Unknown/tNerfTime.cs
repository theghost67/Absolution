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
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

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
            return $"<color>При использовании на любой вражеской карте</color>\n" +
                   $"Удаляет все навыки у цели, понижает инициативу всех союзных карт, кроме себя, на {_moxieF.Format(args.stacks, true)}. Тратит один заряд.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(16, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, TerritoryRange.ownerAllNotSelf).WithCard();

            await trait.SetStacks(0, owner.Side);
            target.Traits.Clear(trait);

            int moxie = _moxieF.ValueInt(e.traitStacks);
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
                await card.Moxie.AdjustValue(-moxie, trait);
        }
    }
}
