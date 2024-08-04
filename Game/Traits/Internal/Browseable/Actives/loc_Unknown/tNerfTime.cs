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
        const int MOXIE_DECREASE_ABS = 2;

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

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на любой вражеской карте",
                    $"Удаляет все навыки у цели, понижает инициацию всех союзных карт, кроме себя, на {MOXIE_DECREASE_ABS} ед. Тратит один заряд."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
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

            target.Traits.Clear(trait);
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
                card.moxie.AdjustValueDefault(-MOXIE_DECREASE_ABS, trait);
            await trait.SetStacks(0, owner.Side);
        }
    }
}
