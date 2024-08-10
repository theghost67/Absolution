using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWeaver : PassiveTrait
    {
        const string ID = "weaver";
        const string CARD_ID = "spider_cocon";
        const string TRAIT_ID = "weaving";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tWeaver() : base(ID)
        {
            name = "Прядильщик";
            desc = "Мне было так одиноко... Пока у меня не появились коконы.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tWeaver(tWeaver other) : base(other) { }
        public override object Clone() => new tWeaver(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении владельца на территории (П{PRIORITY})",
                    $"Расставляет рядом с владельцем карты <i>{cardName}</i> с единицей здоровья и {_stacksF.Format(trait)} " +
                    $"зарядами навыка <i>{traitName}</i>. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(12, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnOwnerFieldPostAttached, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (owner.IsKilled) return;
            if (owner.Field == null) return;

            await trait.AnimActivation();
            await trait.SetStacks(0, trait);
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, _range).WithoutCard();
            foreach (BattleField field in fields)
            {
                FieldCard newCard = CardBrowser.NewField(CARD_ID);
                newCard.traits.AdjustStacks(TRAIT_ID, trait.GetStacks());
                await owner.Territory.PlaceFieldCard(newCard, field, trait);
            }    
        }
    }
}
