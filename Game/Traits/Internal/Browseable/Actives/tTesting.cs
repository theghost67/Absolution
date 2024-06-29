using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tTesting : ActiveTrait
    {
        const string ID = "testing";
        const int MOXIE_THRESHOLD = 0;
        const int DAMAGE = 9999;

        public tTesting() : base(ID)
        {
            name = "Тестирование";
            desc = "Итак, начинаем ТЕСТИРОВАНИЕ.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerSingle, TerritoryRange.oppositeAll);
        }
        protected tTesting(tTesting other) : base(other) { }
        public override object Clone() => new tTesting(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании (Т)",
                    $"тратит все заряды и тестирует карты все карты напротив владельца на прочность - если её инициатива ≤ {MOXIE_THRESHOLD}, ей будет нанесено {DAMAGE} ед. урона."),
            });
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
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, range.splash).WithCard();

            foreach (BattleFieldCard card in fields.Select(f => f.Card))
            {
                if (card.moxie <= MOXIE_THRESHOLD)
                    await card.health.AdjustValue(-DAMAGE, owner);
            }

            trait.SetStacks(0, owner.Side);
        }
    }
}
