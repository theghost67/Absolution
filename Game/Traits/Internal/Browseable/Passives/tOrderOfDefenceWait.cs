using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfDefenceWait : PassiveTrait
    {
        const string ID = "order_of_defence_wait";
        const string TRAIT_ID = "block";
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);

        public tOrderOfDefenceWait() : base(ID)
        {
            name = Translator.GetString("trait_order_of_defence_wait_1");
            desc = Translator.GetString("trait_order_of_defence_wait_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tOrderOfDefenceWait(tOrderOfDefenceWait other) : base(other) { }
        public override object Clone() => new tOrderOfDefenceWait(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_order_of_defence_wait_3", traitName, _stacksF.Format(args.stacks));

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = args.stacks } };
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await trait.Owner.Traits.AdjustStacks(TRAIT_ID, stacks, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
