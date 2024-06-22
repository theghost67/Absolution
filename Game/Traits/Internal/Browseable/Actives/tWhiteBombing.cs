using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tWhiteBombing : ActiveTrait
    {
        const string ID = "white_bombing";
        const string SPAWN_CARD_ID = "pigeon_litter";
        const string SPAWN_TRAIT_ID = "unpleasant_scent";
        const int SPAWN_TRAIT_STACKS = 1;

        public tWhiteBombing() : base(ID)
        {
            name = "Белоснежная бомбардивка";
            desc = "Чёрт, это опять те голуби, бежим отсюда, пока не поздно!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.all);
        }
        protected tWhiteBombing(tWhiteBombing other) : base(other) { }
        public override object Clone() => new tWhiteBombing(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(SPAWN_TRAIT_ID).name;
            string cardName = TraitBrowser.GetTrait(SPAWN_TRAIT_ID).name;
            int effect = SPAWN_TRAIT_STACKS * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При активации на союзном поле (Т)",
                    $"тратит все заряды, создаёт на всех пустых полях своей территории карты <i>Голубиный помёт</i>. Карты будут иметь <u>{effect}</u> зарядов трейта <i>{traitName}</i>."),
                new($"При активации на вражеском поле (Т)",
                    $"тратит все заряды, все карты на территории напротив получат <u>{effect}</u> зарядов трейта <i>{traitName}</i>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 16 + 8 * Mathf.Pow(stacks, 3);
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
                    await trait.Side.Territory.PlaceFieldCard(CardBrowser.NewField(SPAWN_CARD_ID), field, trait.Side);
            }
            else
            {
                IEnumerable<BattleField> fields = trait.Side.Opposite.Fields().WithCard();
                foreach (BattleField field in fields)
                    await field.Card.Traits.Passives.AdjustStacks(SPAWN_TRAIT_ID, SPAWN_TRAIT_STACKS * trait.GetStacks(), trait);
            }

            await trait.SetStacks(0, trait.Side);
        }
    }
}
