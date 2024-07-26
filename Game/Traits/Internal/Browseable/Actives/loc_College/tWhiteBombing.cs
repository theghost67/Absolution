using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWhiteBombing : ActiveTrait
    {
        const string ID = "white_bombing";
        const string SPAWN_CARD_ID = "pigeon_litter";
        const string SPAWN_TRAIT_ID = "unpleasant_scent";

        public tWhiteBombing() : base(ID)
        {
            name = "Белоснежная бомбардировка";
            desc = "Чёрт, это опять те голуби, бежим отсюда, пока не поздно!";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.all);
        }
        protected tWhiteBombing(tWhiteBombing other) : base(other) { }
        public override object Clone() => new tWhiteBombing(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(SPAWN_TRAIT_ID).name;
            int effect = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на союзном поле",
                    $"тратит все заряды, создаёт на всех пустых полях своей территории карты <i>Голубиный помёт</i>. Карты будут иметь <u>{effect}</u> зарядов навыка <i>{traitName}</i>."),
                new($"При использовании на территории на вражеском поле",
                    $"тратит все заряды, инициатива всех карт на территории напротив будет уменьшена на <u>{effect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 12 * Mathf.Pow(stacks - 1, 2);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return new(0, 0.12f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleField target = (BattleField)e.target;
            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            bool usedOnOwnerSide = target.Side.isMe;

            if (usedOnOwnerSide)
            {
                IEnumerable<BattleField> fields = trait.Side.Fields().WithoutCard();
                foreach (BattleField field in fields)
                {
                    FieldCard card = CardBrowser.NewField(SPAWN_CARD_ID);
                    card.traits.Passives.AdjustStacks(SPAWN_TRAIT_ID, trait.GetStacks());
                    await trait.Side.Territory.PlaceFieldCard(card, field, trait.Side);
                }
            }
            else
            {
                IEnumerable<BattleField> fields = trait.Side.Opposite.Fields().WithCard();
                foreach (BattleField field in fields)
                    await field.Card.moxie.AdjustValue(-trait.GetStacks(), trait);
            }

            await trait.SetStacks(0, trait.Side);
        }
    }
}
