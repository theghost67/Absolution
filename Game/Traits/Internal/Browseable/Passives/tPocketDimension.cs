using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPocketDimension : PassiveTrait
    {
        const string ID = "pocket_dimension";

        public tPocketDimension() : base(ID)
        {
            name = Translator.GetString("trait_pocket_dimension_1");
            desc = Translator.GetString("trait_pocket_dimension_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tPocketDimension(tPocketDimension other) : base(other) { }
        public override object Clone() => new tPocketDimension(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_pocket_dimension_3");

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 2, 1.25f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            IEnumerable<BattleField> oppositeFields = owner.Territory.Fields(owner.Field.pos, TerritoryRange.oppositeAll).WithCard();
            BattleField fieldWithMinHp = oppositeFields.FirstOrDefault();
            if (fieldWithMinHp == null) return;
            foreach (BattleField field in oppositeFields)
            {
                if (field.Card.Health < fieldWithMinHp.Card.Health)
                    fieldWithMinHp = field;
            }

            await trait.AnimActivation();
            await trait.Owner.TryAttachToField(fieldWithMinHp.Opposite, trait);
            await trait.AdjustStacks(-1, trait);
        }
    }
}
