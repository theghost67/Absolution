using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWhiteBombing : ActiveTrait
    {
        const string ID = "white_bombing";
        const string CARD_ID = "pigeon_litter";
        const string TRAIT_ID = "unpleasant_scent";
        static readonly TraitStatFormula _stacksMoxieF = new(false, 0, 1);

        public tWhiteBombing() : base(ID)
        {
            name = "Белоснежная бомбардировка";
            desc = "Чёрт, это опять те голуби, бежим отсюда, пока не поздно!";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.bothSingle);
        }
        protected tWhiteBombing(tWhiteBombing other) : base(other) { }
        public override object Clone() => new tWhiteBombing(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            string format = _stacksMoxieF.Format(trait);
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на союзном поле",
                    $"тратит все заряды, создаёт на всех пустых полях своей территории карты <i>{cardName}</i>. Карты будут иметь {format} зарядов навыка <i>{traitName}</i>."),
                new($"При использовании на территории на вражеском поле",
                    $"тратит все заряды, инициатива всех карт на территории напротив будет уменьшена на {format}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(12, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
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
            IBattleTrait trait = (IBattleTrait)e.trait;
            bool usedOnOwnerSide = target.Side.isMe;

            await trait.SetStacks(0, trait.Side);
            if (usedOnOwnerSide)
            {
                IEnumerable<BattleField> fields = trait.Side.Fields().WithoutCard();
                foreach (BattleField field in fields)
                {
                    FieldCard card = CardBrowser.NewField(CARD_ID);
                    card.traits.Passives.AdjustStacks(TRAIT_ID, _stacksMoxieF.ValueInt(trait));
                    await trait.Side.Territory.PlaceFieldCard(card, field, trait.Side);
                }
            }
            else
            {
                IEnumerable<BattleField> fields = trait.Side.Opposite.Fields().WithCard();
                foreach (BattleField field in fields)
                    await field.Card.Moxie.AdjustValue(-_stacksMoxieF.ValueInt(trait), trait);
            }
        }
    }
}
