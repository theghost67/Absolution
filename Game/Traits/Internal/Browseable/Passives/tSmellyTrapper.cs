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
    public class tSmellyTrapper : PassiveTrait
    {
        const string ID = "smelly_trapper";
        const string CARD_ID = "crap";

        public tSmellyTrapper() : base(ID)
        {
            name = Translator.GetString("trait_smelly_trapper_1");
            desc = Translator.GetString("trait_smelly_trapper_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tSmellyTrapper(tSmellyTrapper other) : base(other) { }
        public override object Clone() => new tSmellyTrapper(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_smelly_trapper_3", cardName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(5, stacks, 1, 1.10f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
            }
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null || owner.IsKilled) return;

            BattleField[] fields = territory.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithoutCard().ToArray();
            if (fields.Length == 0) return;

            await trait.AnimActivation();
            foreach (BattleField field in fields)
            {
                FieldCard newCard = CardBrowser.NewField(CARD_ID);
                await territory.PlaceFieldCard(newCard, field, trait);
            }
            await trait.AdjustStacks(-1, trait);
        }

        async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || owner.IsKilled) return;

            BattleTerritory territory = owner.Territory;
            BattleField[] fields = territory.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithoutCard().ToArray();
            if (fields.Length == 0) return;

            await trait.AnimActivation();
            foreach (BattleField field in fields)
            {
                FieldCard newCard = CardBrowser.NewField(CARD_ID);
                await territory.PlaceFieldCard(newCard, field, trait);
            }
            await trait.AdjustStacks(-1, trait);
        }
    }
}
