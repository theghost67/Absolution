﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tZenSchool : ActiveTrait
    {
        const string ID = "zen_school";

        public tZenSchool() : base(ID)
        {
            name = "Школа дзена";
            desc = "Познай путь дзена, брат мой.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tZenSchool(tZenSchool other) : base(other) { }
        public override object Clone() => new tZenSchool(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на любой союзной карте",
                    $"перенаправляет всю силу карты в её здоровье. Так же восстанавливает своё здоровье на то же значение."),
            });
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null && e.target.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleFieldCard card = (BattleFieldCard)e.target.Card;
            int strength = card.strength;

            await card.strength.AdjustValue(-strength, e.trait);
            await card.health.AdjustValue(strength, e.trait);
            await e.trait.Owner.health.AdjustValue(strength, e.trait);
        }
    }
}