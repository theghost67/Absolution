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
    public class tTesting : ActiveTrait
    {
        const string ID = "testing";
        const int MOXIE_THRESHOLD = 0;
        const int DAMAGE = 9999;
        static readonly TerritoryRange targets = TerritoryRange.oppositeAll;

        public tTesting() : base(ID)
        {
            name = "Тестирование";
            desc = "Итак, начинаем ТЕСТИРОВАНИЕ.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tTesting(tTesting other) : base(other) { }
        public override object Clone() => new tTesting(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории",
                    $"Тестирует карты все карты напротив владельца на прочность - если её инициатива ≤ {MOXIE_THRESHOLD}, ей будет нанесено {DAMAGE} ед. урона. Тратит все заряды."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
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

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, targets).WithCard();
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
            {
                if (card.moxie <= MOXIE_THRESHOLD)
                    await card.health.AdjustValue(-DAMAGE, owner);
            }
            trait.SetStacks(0, owner.Side);
        }
    }
}
