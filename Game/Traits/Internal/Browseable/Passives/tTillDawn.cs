using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTillDawn : PassiveTrait
    {
        const string ID = "till_dawn";
        static readonly TraitStatFormula _healthF = new(true, 0, 0.33f);
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tTillDawn() : base(ID)
        {
            name = Translator.GetString("trait_till_dawn_1");
            desc = Translator.GetString("trait_till_dawn_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tTillDawn(tTillDawn other) : base(other) { }
        public override object Clone() => new tTillDawn(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_till_dawn_3", _healthF.Format(args.stacks), _moxieF.Format(args.stacks));

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
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await owner.Health.AdjustValue(owner.Data.health * _healthF.Value(stacks), trait);
            await owner.Moxie.AdjustValue(_moxieF.Value(stacks), trait);
            await trait.SetStacks(0, trait);
        }
    }
}
